using Content.Shared._Finster.Rulebook;
using Robust.Shared.GameStates;

namespace Content.Shared._Finster.Rulebook.Events;

/// <summary>
/// Raised when attempting any type of skill/attribute check.
/// Handlers can modify the outcome.
/// </summary>
[ByRefEvent]
public record struct SkillCheckEvent(
    EntityUid User,
    int TargetValue,
    int Bonus,
    int ToolBonus,
    bool CriticalSuccess,
    bool CriticalFailure,
    bool Handled = false,
    bool Result = false
);

/// <summary>
/// Raised when attempting an attribute check.
/// </summary>
[ByRefEvent]
public record struct AttributeCheckEvent(
    EntityUid User,
    AttributeType Attribute,
    int Bonus,
    int ToolBonus,
    bool CriticalSuccess,
    bool CriticalFailure,
    bool Handled = false,
    bool Result = false
) : ISkillCheckEvent;

/// <summary>
/// Raised when attempting a skill check.
/// </summary>
[ByRefEvent]
public record struct SkillTypeCheckEvent(
    EntityUid User,
    SkillType Skill,
    int SituationalBonus,
    int ToolBonus,
    bool CriticalSuccess,
    bool CriticalFailure,
    bool Handled = false,
    bool Result = false
) : ISkillCheckEvent;

public interface ISkillCheckEvent
{
    public EntityUid User { get; }
    public bool CriticalSuccess { get; set; }
    public bool CriticalFailure { get; set; }
    public bool Handled { get; set; }
    public bool Result { get; set; }
}