using Work_IA.Application;
using Work_IA.Application.Services;
using Work_IA.Infrastructure;
using Work_IA.Infrastructure.Persistence;
using Work_IA.WebApi.Hubs;
using Work_IA.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<MetricsService>();

var app = builder.Build();

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

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<AgentCommunicationHub>("/hub/agents");

app.Run();
