using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using LinenManagementSystem.Data;
using LinenManagementSystem.Services;
using LinenManagementSystem.Repositories;
using System.Text;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Logging setup
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Add console logging

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Linen Management System API", Version = "v1" });

    // Add JWT support to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token.\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Entity Framework and DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your services and repositories
builder.Services.AddScoped<ICartLogRepository, CartLogRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICartLogService, CartLogService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Set to true if you specify an issuer
        ValidateAudience = false, // Set to true if you specify an audience
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero // Ensure token expiration is exact
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask; // Response already started, do not modify
            }

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\":\"Authentication failed: " + context.Exception.Message + "\"}");
        }
    };

});


// Configure Kestrel for HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7199, listenOptions =>
    {
        listenOptions.UseHttps(); // Ensure HTTPS is enabled
    });
});

// Add controller services
builder.Services.AddControllers();
builder.Services.AddAuthorization(); 

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Linen Management System API v1"));
}

// Optional: Uncomment if using custom middleware for header validation
// app.UseMiddleware<TokenValidationMiddleware>(); 



app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

// Use authentication and authorization middleware
app.UseRouting();
app.UseMiddleware<TokenValidationMiddleware>(); 
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalErrorHandlerMiddleware>();

app.MapControllers(); // Map attribute routes

app.Run();
