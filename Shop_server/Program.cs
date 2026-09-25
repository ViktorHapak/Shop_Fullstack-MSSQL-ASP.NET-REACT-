using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shop_server.Data;
using Shop_server.Middleware;
using Shop_server.Security;
using Shop_server.Services;
using Shop_server.util;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(new
        {
            status = StatusCodes.Status400BadRequest,
            error = "Invalid request body",
            message = "Érvénytelen adat-formátum."
        });
    };
})
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var connectionString =
    builder.Configuration.GetConnectionString("ShopConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'ShopConnection' was not found.");

builder.Services.AddTransient<RequestFilterMiddleware>();
builder.Services.AddTransient<TokenFilterMiddleware>();

builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<AuthoritiesDAO>();
builder.Services.AddScoped<SoldDAO>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };
    });

builder.Services.AddScoped<JwtUnit>();
builder.Services.AddScoped<UserDetailsService>();
builder.Services.AddScoped<PasswordHasher>();

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ReactClient");

app.UseAuthentication();

app.UseMiddleware<TokenFilterMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<RequestFilterMiddleware>();

app.Run();

