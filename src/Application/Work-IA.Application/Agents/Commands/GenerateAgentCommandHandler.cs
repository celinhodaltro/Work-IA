using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Application.Services;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class GenerateAgentCommandHandler : IRequestHandler<GenerateAgentCommand, AgentId>
{
    private readonly IAgentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AgentRegistry _agentRegistry;
    private readonly IEventBus _eventBus;
    private readonly IMediator _mediator;
    private readonly IOpenCodeService _openCode;
    private readonly ILogger<GenerateAgentCommandHandler> _logger;

    public GenerateAgentCommandHandler(
        IAgentRepository repository,
        IUnitOfWork unitOfWork,
        AgentRegistry agentRegistry,
        IEventBus eventBus,
        IMediator mediator,
        IOpenCodeService openCode,
        ILogger<GenerateAgentCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _agentRegistry = agentRegistry;
        _eventBus = eventBus;
        _mediator = mediator;
        _openCode = openCode;
        _logger = logger;
    }

    public async Task<AgentId> Handle(GenerateAgentCommand request, CancellationToken ct)
    {
        var levelName = request.Level switch
        {
            AgentCareerLevel.Intern => "Intern",
            AgentCareerLevel.Junior => "Junior",
            AgentCareerLevel.Pleno => "Pleno",
            AgentCareerLevel.Senior => "Senior",
            AgentCareerLevel.TechLead => "Tech Lead",
            AgentCareerLevel.Architect => "Architect",
            AgentCareerLevel.Head => "Head",
            _ => "Junior"
        };

        var prompt = new OpenCodePrompt("", new Dictionary<string, string>
        {
            ["NAME"] = request.Name,
            ["TITLE"] = request.Title,
            ["LEVEL"] = levelName
        });

        var result = await _openCode.ExecutePromptAsync(prompt, ct);
        if (!result.Success)
        {
            _logger.LogWarning("OpenCode failed, creating agent with defaults: {Error}", result.Error);
            return await CreateWithDefaults(request);
        }

        var (skills, personality) = TryExtractProfile(result.ExtractedJson ?? result.RawOutput);
        var agent = Agent.Create(new AgentName(request.Name), new AgentTitle(request.Title));

        foreach (var skill in skills)
            agent.AssignSkill(new AgentSkill(skill));

        await _repository.AddAsync(agent, ct);

        var runtimeAgent = new AgentBase(agent, _eventBus, _mediator, _logger);
        _agentRegistry.Register(runtimeAgent);

        await SaveLocalAgentFile(agent, personality, levelName);

        return agent.AgentId;
    }

    private async Task<AgentId> CreateWithDefaults(GenerateAgentCommand request)
    {
        var agent = Agent.Create(new AgentName(request.Name), new AgentTitle(request.Title));
        agent.AssignSkill(new AgentSkill("Communication"));
        agent.AssignSkill(new AgentSkill("Problem Solving"));

        await _repository.AddAsync(agent, CancellationToken.None);

        var runtimeAgent = new AgentBase(agent, _eventBus, _mediator, _logger);
        _agentRegistry.Register(runtimeAgent);

        return agent.AgentId;
    }

    private static (List<string> Skills, string? Personality) TryExtractProfile(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var skills = new List<string>();

            if (root.TryGetProperty("skills", out var skillsEl))
            {
                foreach (var s in skillsEl.EnumerateArray())
                    skills.Add(s.GetString() ?? "");
            }

            var personality = root.TryGetProperty("personality", out var p) ? p.GetString() : null;
            return (skills, personality);
        }
        catch
        {
            return (new List<string> { "Communication", "Problem Solving" }, null);
        }
    }

    private static async Task SaveLocalAgentFile(Agent agent, string? personality, string levelName)
    {
        try
        {
            var agentsDir = Path.Combine(Directory.GetCurrentDirectory(), ".work-ia", "agents");
            Directory.CreateDirectory(agentsDir);

            var data = new
            {
                Id = agent.AgentId.Value,
                Name = agent.Name.Value,
                Title = agent.Title.Value,
                Level = levelName,
                Personality = personality ?? "Professional",
                Skills = agent.Skills.Select(s => s.Name).ToList(),
                CreatedAt = agent.JoinedAt
            };

            var filePath = Path.Combine(agentsDir, $"{agent.AgentId.Value:N}.json");
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);

            var gitignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".gitignore");
            var entry = ".work-ia/";
            if (File.Exists(gitignorePath))
            {
                var content = await File.ReadAllTextAsync(gitignorePath);
                if (!content.Contains(entry))
                    await File.AppendAllTextAsync(gitignorePath, $"\n# Work-IA local agents\n{entry}\n");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save local agent file: {ex.Message}");
        }
    }
}
