using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Work_IA.Application;
using Work_IA.Infrastructure;
using Work_IA.Infrastructure.Persistence;
using Work_IA.WebApi.Authorization;
using Work_IA.WebApi.Hubs;
using Work_IA.WebApi.Middleware;
using Work_IA.WebApi.Serialization; using Work_IA.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var jwtSecret = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(jwtSecret) || jwtSecret == "default-secret-key-change-me")
{
    throw new InvalidOperationException("JWT Secret must be configured and not the default value.");
}
var jwtKey = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPermissionPolicies();
});

builder.Services.AddControllers();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new AgentIdJsonConverter());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new AgentIdJsonConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// CORS - permitir qualquer origem em desenvolvimento
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WorkIaDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// MAPEAR ROTAS PRIMEIRO
app.MapControllers();
app.MapHub<AgentCommunicationHub>("/hub/agents"); app.MapHub<AgentStateHub>("/hub/agent-states");

// DEPOIS servir arquivos estÃ¡ticos (fallback)
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");

builder.Services.AddHostedService<AgentBehaviorHostedService>(); app.Run();
