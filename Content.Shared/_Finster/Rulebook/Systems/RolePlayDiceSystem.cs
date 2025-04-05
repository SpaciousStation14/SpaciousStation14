using Robust.Shared.Random;

namespace Content.Shared._Finster.Rulebook;


/// <summary>
/// No, it's not like SS14's DiceSystem. It is internal system for another systems calculation.
///
/// NOTES: We should use 1d20 for attack checking. 2d4, 2d6, 2d8 and etc. for skills check.
/// </summary>
public sealed class RolePlayDiceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Roll dice and try your luck!
    /// </summary>
    /// <param name="diceType">The dice what should be thrown.</param>
    /// <param name="modifier">Apply modifiers.</param>
    /// <param name="isCriticalSuccess"></param>
    /// <param name="isCriticalFailure"></param>
    /// <returns></returns>
    public int Roll(
        DiceType diceType,
        out bool isCriticalSuccess,
        out bool isCriticalFailure,
        int modifier = 0)
    {
        isCriticalSuccess = false;
        isCriticalFailure = false;

        var sides = (int) diceType;
        var result = _random.Next(1, sides + 1);

        if (result == sides)
        {
            isCriticalSuccess = true;
            return result;
        }
        else if (result == 1) // critical fail
        {
            isCriticalFailure = true;
            return result;
        }

        return result + modifier;
    }
}

/// <summary>
/// Holy...
/// </summary>
public enum DiceType
{
    D4 = 4,
    D6 = 6,
    D8 = 8,
    D10 = 10,
    D12 = 12,
    D20 = 20,
    D100 = 100
}
