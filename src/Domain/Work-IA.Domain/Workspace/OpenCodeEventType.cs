namespace Work_IA.Domain.Workspace;

public enum OpenCodeEventType
{
    FileCreated,
    FileChanged,
    FileDeleted,
    ConversationStarted,
    ToolStarted,
    ToolCompleted,
    PromptSent,
    ResponseReceived,
    TestStarted,
    TestCompleted,
    GitCommit,
    GitPush,
    ErrorOccurred,
    BuildCompleted,
    DeployCompleted
}
