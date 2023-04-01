using System.Configuration;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using TodoLists.App.Models;
using TodoLists.App.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("TodoLists.App.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Start.");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddDbContext<TodoContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("TodoContext"));
        options.UseSnakeCaseNamingConvention();
    });
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "Standard Authorization header using Bearer scheme (\"Bearer {token}\").",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
        });
        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });

    var jwtKey = builder.Configuration.GetSection("AppSettings:JwtKey").Value ??
                 throw new ConfigurationErrorsException("Required configuration option AppSettings:JwtKey is not set.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    Log.Information("Ignored HostAbortedException");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to init the application");
}
finally
{
    Log.CloseAndFlush();
}

Log.Information("Exited gracefully.");

