using System.Configuration;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using TodoLists.App.Entities;
using TodoLists.App.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("TodoLists.App.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 8)
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Start");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .WriteTo.Console()
            .WriteTo.File(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "../TodoLists.App.log" : "TodoLists.App.log",
                rollingInterval: RollingInterval.Day);
    });

    builder.Services.AddControllers();
    builder.Services.AddDbContext<TodoListsDbContext>(options =>
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

    string wwwrootDir;
    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
    if (app.Environment.IsDevelopment())
    {
        wwwrootDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../wwwroot"));
    }
    else
    {
        wwwrootDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
    }

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(wwwrootDir),
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    Log.Information("Completed configuring ASP.NET app");
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

Log.Information("Exited gracefully");

