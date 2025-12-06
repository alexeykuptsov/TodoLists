using System.Configuration;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Sinks.SystemConsole.Themes;
using Swashbuckle.AspNetCore.Filters;
using TodoLists.App.Entities;
using TodoLists.App.Middleware;
using TodoLists.App.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Grayscale)
    .WriteTo.File(
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "../TodoLists.App.log" : "TodoLists.App.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Start");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

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

    // Add CORS configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowVueApp", policy =>
        {
            policy.WithOrigins("http://localhost:8080", "https://localhost:8080")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
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

    app.UseMiddleware<ExceptionMiddleware>();
    
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(wwwrootDir),
    });

    app.UseHttpsRedirection();

    // Enable CORS
    app.UseCors("AllowVueApp");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    Log.Information("Completed configuring ASP.NET app");
    app.Run();
}
catch (HostAbortedException)
{
    // Ignored HostAbortedException - ;
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

