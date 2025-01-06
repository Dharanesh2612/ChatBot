//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllers();

//// Add CORS support to allow React app running on http://localhost:3000
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp", builder =>
//    {
//        builder.WithOrigins("http://localhost:3000")  // Allow requests from the React app
//               .AllowAnyMethod()                    // Allow any HTTP method (GET, POST, etc.)
//               .AllowAnyHeader()                    // Allow any headers
//               .AllowCredentials();                 // Allow credentials (cookies, authorization headers)
//    });
//});

//// Add Swagger/OpenAPI support
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Enable CORS middleware to allow cross-origin requests
//app.UseCors("AllowReactApp"); // Apply CORS here

//// Enable Swagger in development
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//// Enable HTTPS redirection
//app.UseHttpsRedirection();

//// Authorization middleware
//app.UseAuthorization();

//// Map controllers to routes
//app.MapControllers();

//// Run the app
//app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Allow requests from the React app
              .AllowAnyMethod()                   // Allow all HTTP methods (GET, POST, etc.)
              .AllowAnyHeader()                   // Allow any headers
              .AllowCredentials();                // Allow cookies or credentials if needed
    });
});

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS middleware
app.UseCors("AllowReactApp"); // Apply the "AllowReactApp" CORS policy

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Add Authorization middleware
app.UseAuthorization();

// Map controllers to routes
app.MapControllers();

// Run the application
app.Run();

