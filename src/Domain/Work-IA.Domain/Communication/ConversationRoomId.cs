namespace Work_IA.Domain.Communication;

public readonly record struct ConversationRoomId(Guid Value)
{
    public static ConversationRoomId New() => new(Guid.NewGuid());
    public static ConversationRoomId From(Guid value) => new(value);
}
