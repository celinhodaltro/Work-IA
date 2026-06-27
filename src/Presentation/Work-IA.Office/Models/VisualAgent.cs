namespace Work_IA.Office.Models;

public sealed class VisualAgent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Created";
    public string Emotion { get; set; } = "Neutral";
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }
    public int Level { get; set; }
    public int Xp { get; set; }
    public bool IsInMeeting { get; set; }
    public float CurrentX { get; set; }
    public float CurrentY { get; set; }
    public float ConversationTimer { get; set; }
    public string? ConversingWith { get; set; }

    public void Update(float deltaTime)
    {
        var speed = 50f * deltaTime;
        var dx = TargetX - CurrentX;
        var dy = TargetY - CurrentY;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist > 1)
        {
            CurrentX += (dx / dist) * speed;
            CurrentY += (dy / dist) * speed;
        }
        else
        {
            CurrentX = TargetX;
            CurrentY = TargetY;
        }
        PositionX = CurrentX;
        PositionY = CurrentY;
    }

    public string GetEmoji()
    {
        return Emotion switch
        {
            "Happy" => "😊",
            "Tired" => "😰",
            "Excited" => "🎉",
            "Stressed" => "😫",
            "Celebrating" => "🥳",
            "Thinking" => "🤔",
            _ => "😐"
        };
    }
}
