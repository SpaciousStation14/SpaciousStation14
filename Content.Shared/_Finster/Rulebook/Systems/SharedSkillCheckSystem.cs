using Content.Shared._Finster.Rulebook;
using Content.Shared._Finster.Rulebook.Events;
using Robust.Shared.Random;

public sealed class SharedSkillCheckSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// Perform an attribute check by raising an event
    /// </summary>
    public bool TryAttributeCheck(
        EntityUid user,
        AttributeType attribute,
        out bool criticalSuccess,
        out bool criticalFailure,
        int bonus = 0)
    {
        var ev = new AttributeCheckEvent(
            User: user,
            Attribute: attribute,
            Bonus: bonus,
            CriticalSuccess: false,
            CriticalFailure: false
        );

        RaiseLocalEvent(ref ev);

        criticalSuccess = ev.CriticalSuccess;
        criticalFailure = ev.CriticalFailure;
        return ev.Result;
    }

    /// <summary>
    /// Perform a skill check by raising an event
    /// </summary>
    public bool TrySkillCheck(
        EntityUid user,
        SkillType skill,
        out bool criticalSuccess,
        out bool criticalFailure,
        int situationalBonus = 0)
    {
        var ev = new SkillTypeCheckEvent(
            User: user,
            Skill: skill,
            SituationalBonus: situationalBonus,
            CriticalSuccess: false,
            CriticalFailure: false
        );

        RaiseLocalEvent(ref ev);

        criticalSuccess = ev.CriticalSuccess;
        criticalFailure = ev.CriticalFailure;
        return ev.Result;
    }

    /// <summary>
    /// Core rolling logic that can be called by event handlers
    /// </summary>
    public bool TryRawCheck(
        EntityUid user,
        int targetValue,
        int bonus,
        out bool criticalSuccess,
        out bool criticalFailure)
    {
        criticalSuccess = false;
        criticalFailure = false;

        // Roll 3d6
        var roll1 = _random.Next(1, 7);
        var roll2 = _random.Next(1, 7);
        var roll3 = _random.Next(1, 7);
        var total = roll1 + roll2 + roll3 + bonus;
        var unmodifiedTotal = roll1 + roll2 + roll3;

        // Critical checks
        criticalFailure = unmodifiedTotal == 17 || unmodifiedTotal == 18;
        criticalSuccess = unmodifiedTotal == 3 || unmodifiedTotal == 4;

        if (criticalSuccess) return true;
        if (criticalFailure) return false;

        return total <= targetValue;
    }
}