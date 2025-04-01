namespace Content.Shared._Finster.Rulebook;

[RegisterComponent]
public sealed partial class ToolQualityComponent : Component
{
    [DataField("qualities")]
    public HashSet<string> Qualities = new();
}