using CompanyEmployee.Utility;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.Presentation.ActionFilters;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using NLog;
using Service.DataShaping;
using Shared.DataTransferObjects;

var builder = WebApplication.CreateBuilder(args);

// Configure NLog
LogManager.Setup()
    .LoadConfigurationFromFile(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"))
    .GetCurrentClassLogger();

builder.Services.ConfigureCors(); // Configure CORS policies
builder.Services.ConfigureIISIntegration(); // IIS integration settings
builder.Services.ConfigureLoggerService(); // Custom logger service
builder.Services.ConfigureRepositoryManager(); // Add repository manager
builder.Services.ConfigureServiceManager(); // Add service manager
builder.Services.ConfigureSqlContext(builder.Configuration); // Configure SQL context

// Add AutoMapper with the current assembly
builder.Services.AddAutoMapper(typeof(Program));

// Suppress default model state validation filter
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Add scoped services
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();

// Configure API versioning
builder.Services.ConfigureVersioning();

// Enable response caching and HTTP cache headers
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();



// Configure MVC and controllers
builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true; // Respect Accept header
    config.ReturnHttpNotAcceptable = false; // Return 406 for unsupported media types
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile
    {
        Duration = 120 // Cache duration in seconds
    });
})
    .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly) // Add presentation assembly
    .AddNewtonsoftJson() // Add support for JSON with Newtonsoft
    .AddXmlDataContractSerializerFormatters() // Add XML support
    .AddMvcOptions(options =>
    {
        options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
    });
// Add custom media types for the API
builder.Services.AddCustomMediaTypes();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);

if (app.Environment.IsProduction())
{
    app.UseHsts(); // Use HSTS in production
}

app.UseStaticFiles(); // Serve static files

// Configure forwarded headers for proxy support
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors("CorsPolicy"); // Apply CORS policy

// Enable caching middleware
app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseAuthorization(); // Authorization middleware

// Map controllers
app.MapControllers();

// Run the application
app.Run();
