using Robust.Shared.GameStates;

namespace Content.Shared._Finster.Rulebook;

/// <summary>
/// All statistics from the character for using role/dice systems.
/// It also can be applied for the another entity, if we wanna use RolePlay mechanics on them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StatisticsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<AttributeType, int> Attributes { get; set; } = new();

    [DataField, AutoNetworkedField]
    public Dictionary<SkillType, int> Skills { get; set; } = new();

    /// <summary>
    /// Gets an attribute's value, or returns a default (10) if not found.
    /// </summary>
    public int GetAttributeValue(AttributeType attribute)
    {
        return Attributes.TryGetValue(attribute, out var value) ? value : 10;
    }

    /// <summary>
    /// Gets an attribute's value, or returns a default (10) if not found.
    /// </summary>
    public int GetSkillValue(SkillType skill)
    {
        return Skills.TryGetValue(skill, out var value) ? value : 0;
    }

    public int GetSkillModifier(SkillType skill, int skillLevel)
    {
        // Untrained penalty (overrides everything)
        if (skillLevel == 0)
            return -4;

        var difficulty = skill.GetDifficulty();
        var baseAttribute = skill.GetBaseAttribute();

        return difficulty switch
        {
            SkillDifficulty.Easy => CalculateEasySkill(skillLevel),
            SkillDifficulty.Average => CalculateAverageSkill(skillLevel),
            SkillDifficulty.Hard => CalculateHardSkill(skillLevel),
            SkillDifficulty.VeryHard => CalculateVeryHardSkill(skillLevel),
            _ => CalculateEasySkill(skillLevel)
        };
    }

    private int CalculateEasySkill(int level)
    {
        if (level == 1) return 0;
        if (level == 2) return 2;
        return 2 + ((level - 2) / 2); // +1 per 2 levels after 2
    }

    private int CalculateAverageSkill(int level)
    {
        if (level == 1) return -1;
        return -1 + level; // +1 per level after 1
    }

    private int CalculateHardSkill(int level)
    {
        if (level == 1) return -2;
        return -2 + (level - 1); // +1 per level after 1
    }

    private int CalculateVeryHardSkill(int level)
    {
        if (level == 1) return -3;
        return -3 + (level - 1); // +1 per level after 1
    }

    /// <summary>
    /// Calculate modifier, given by the attribute.
    /// </summary>
    /// <param name="attributeValue">Attribute?</param>
    /// <returns></returns>
    public int GetModifier(int attributeValue)
    {
        return (attributeValue - 10) / 2;
    }
}

public enum AttributeType
{
    /// <summary>
    /// It can help you to deal more damage. Also it help to control fire from
    /// ranged weapon, and another calculations.
    /// </summary>
    Strength,

    /// <summary>
    /// Dodge attacks from the enemies! It help to deal aimed damage without
    /// missing attacks (in melee combat).
    /// </summary>
    Dexterity,

    /// <summary>
    /// Heal yourself, heal your allies, use computers.
    /// </summary>
    Intelligence,

    /// <summary>
    /// Withstand PSYCHIC ATTACKS, make your mind a fortress.
    /// </summary>
    Will,
}

public enum SkillType
{
    /// <summary>
    /// It's First Aid. You should know what this is.
    /// ranged weapon, and another calculations.
    /// </summary>
    [SkillDifficulty(SkillDifficulty.Easy)]
    [BaseAttribute(AttributeType.Intelligence)]
    FirstAid,

    /// <summary>
    /// Diagnosing Injuries. Using Medical equipment.
    /// missing attacks (in melee combat).
    /// </summary>
    [SkillDifficulty(SkillDifficulty.Average)]
    [BaseAttribute(AttributeType.Intelligence)]
    Diagnosis,
}