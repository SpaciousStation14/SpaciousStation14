using Content.Shared._Finster.Rulebook;
using Robust.Shared.Random;
public sealed class SharedSkillCheckSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <summary>
    /// Pure attribute check (no skill modifiers)
    /// </summary>
    public bool TryAttributeCheck(
        EntityUid user,
        AttributeType attribute,
        out bool criticalSuccess,
        out bool criticalFailure,
        int bonus = 0)
    {
        // Same as your current implementation
        return TryRawCheck(
            user: user,
            getTargetValue: stats => stats.GetAttributeValue(attribute),
            bonus: bonus,
            out criticalSuccess,
            out criticalFailure
        );
    }

    /// <summary>
    /// Skill-modified check
    /// </summary>
    public bool TrySkillCheck(
        EntityUid user,
        SkillType skill,
        out bool criticalSuccess,
        out bool criticalFailure,
        int situationalBonus = 0)
    {
        return TryRawCheck(
            user: user,
            getTargetValue: stats => {
                var attribute = skill.GetBaseAttribute();
                var skillLevel = stats.Skills.GetValueOrDefault(skill);
                var difficulty = skill.GetDifficulty();

                return stats.GetAttributeValue(attribute)
                       + GetSkillModifier(skillLevel, difficulty);
            },
            bonus: 0, // Already factored into targetValue
            out criticalSuccess,
            out criticalFailure
        );
    }

    /// <summary>
    /// Core rolling logic shared by both checks
    /// </summary>
    private bool TryRawCheck(
    EntityUid user,
    Func<StatisticsComponent, int> getTargetValue, // Renamed from targetValueGetter
    int bonus,
    out bool criticalSuccess,
    out bool criticalFailure)
    {
        criticalSuccess = false;
        criticalFailure = false;

        if (!TryComp<StatisticsComponent>(user, out var stats))
            return false;

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

        var targetValue = getTargetValue(stats); // Now matches parameter name
        return total <= targetValue;
    }

    private int GetSkillModifier(int skillLevel, SkillDifficulty difficulty)
    {
        if (skillLevel == 0) return -4; // Untrained penalty

        return difficulty switch
        {
            SkillDifficulty.Easy when skillLevel == 1 => 0,
            SkillDifficulty.Easy when skillLevel == 2 => 2,
            SkillDifficulty.Easy => 2 + (skillLevel - 2) / 2,

            SkillDifficulty.Average when skillLevel == 1 => -1,
            SkillDifficulty.Average => -1 + skillLevel,

            SkillDifficulty.Hard when skillLevel == 1 => -2,
            SkillDifficulty.Hard => -2 + (skillLevel - 1),

            SkillDifficulty.VeryHard when skillLevel == 1 => -3,
            SkillDifficulty.VeryHard => -3 + (skillLevel - 1),

            _ => 0
        };
    }
}