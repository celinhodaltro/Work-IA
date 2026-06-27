using Microsoft.AspNetCore.Authorization;

namespace Work_IA.WebApi.Authorization;

public static class PermissionPolicies
{
    public const string WorkspaceObserve = "WorkspaceObserve";
    public const string WorkspaceRead = "WorkspaceRead";
    public const string TasksExecute = "TasksExecute";
    
    public static void AddPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(WorkspaceObserve, policy =>
            policy.RequireClaim("permissions", "workspace:observe"));
        
        options.AddPolicy(WorkspaceRead, policy =>
            policy.RequireClaim("permissions", "workspace:read"));
        
        options.AddPolicy(TasksExecute, policy =>
            policy.RequireClaim("permissions", "tasks:execute"));
    }
}
