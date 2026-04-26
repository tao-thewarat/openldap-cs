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
builder.Services.AddScoped<ILdapDirectoryService, LdapDirectoryService>();
builder.Services.AddScoped<ILdapAuthService, LdapAuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
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
