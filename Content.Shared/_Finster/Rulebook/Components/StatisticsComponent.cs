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
    /// Affect on communication skills, understading and learning something new.
    /// </summary>
    Intelligence,

    /// <summary>
    /// Reduce any damage, like physical. It also can reduce time in crit, when your death is become.
    /// </summary>
    Endurance,

    /// <summary>
    /// Tt help to target into body parts on ranged distance with the
    /// ranged weapons and guns.
    /// </summary>
    Perception
}

public enum SkillType
{
    /// <summary>
    /// How good are you can use melee weapons. Mostly affected by Dexterity.
    /// </summary>
    Melee,

    /// <summary>
    /// How good you can use ranged weapons. Mostly affected by Strength for control & Perception for aim.
    /// </summary>
    Ranged,

    /// <summary>
    /// How good you can apply first aid help to anyone. Mostly affected by Intelligence.
    /// </summary>
    Medicine,

    /// <summary>
    /// How good you know how to construct and craft any items. Mostly affected by Intelligence.
    /// </summary>
    Engineering,

    /// <summary>
    /// Try to dodge the melee attacks. Mostly affected by Dexterity.
    /// </summary>
    //Dodging,

    /// <summary>
    /// How good you can deal damage by your bare hands. Mostly affected by Strength.
    /// </summary>
    //MartialArts,
}

public enum SkillLevelType
{
    Weak = DiceType.D4,
    Normal = DiceType.D6,
    Good = DiceType.D8,
    Expert = DiceType.D10,
    Master = DiceType.D12,
    Legendary = DiceType.D20
}
