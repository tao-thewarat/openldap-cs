using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenLdapCs.Common.Routing;
using OpenLdapCs.Interfaces;
using OpenLdapCs.Options;
using OpenLdapCs.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<LdapOptions>(builder.Configuration.GetSection(LdapOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName)
);

var jwtOptions =
    builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

var redisOptions =
    builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
    ?? throw new InvalidOperationException("Redis configuration is missing.");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisOptions.Configuration;
    options.InstanceName = redisOptions.InstanceName;
});

builder.Services.AddScoped<ILdapDirectoryService, LdapDirectoryService>();
builder.Services.AddScoped<ILdapAuthService, LdapAuthService>();
builder.Services.AddScoped<IRefreshTokenStore, RedisRefreshTokenStore>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)
            ),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (
                    context.Request.Cookies.TryGetValue(
                        jwtOptions.AccessTokenCookieName,
                        out var token
                    )
                )
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers(options =>
{
    options.Conventions.Insert(0, new RoutePrefixConvention(new RouteAttribute("api/v1")));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

app.Run();
