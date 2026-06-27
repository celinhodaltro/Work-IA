namespace Work_IA.Application.Agents.Roles;

public interface IReviewerAgent
{
    bool CanBlockDelivery(string reason);
}
