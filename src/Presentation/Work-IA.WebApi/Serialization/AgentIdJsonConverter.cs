using System.Text.Json;
using System.Text.Json.Serialization;
using Work_IA.Domain.Agents;

namespace Work_IA.WebApi.Serialization;

public sealed class AgentIdJsonConverter : JsonConverter<AgentId>
{
    public override AgentId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new AgentId(reader.GetGuid());
    }

    public override void Write(Utf8JsonWriter writer, AgentId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
