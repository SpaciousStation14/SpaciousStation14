using Robust.Shared.Random;

namespace Content.Shared._Finster.Rulebook;


/// <summary>
/// No, it's not like SS14's DiceSystem. It is internal system for another systems calculation.
/// </summary>
public sealed class RolePlayDiceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Roll 3d6 and try your luck!
    /// </summary>
    /// <param name="modifier">Apply modifiers to the roll.</param>
    /// <param name="isCriticalSuccess">True if roll is 17 or 18.</param>
    /// <param name="isCriticalFailure">True if roll is 3 or 4.</param>
    /// <returns>The sum of 3d6 plus modifier</returns>
    public int Roll(
        out bool isCriticalSuccess,
        out bool isCriticalFailure,
        int modifier = 0)
    {
        isCriticalSuccess = false;
        isCriticalFailure = false;

        // Roll 3d6
        var roll1 = _random.Next(1, 7);
        var roll2 = _random.Next(1, 7);
        var roll3 = _random.Next(1, 7);
        var total = roll1 + roll2 + roll3 + modifier;

        // Check for critical results
        var unmodifiedTotal = roll1 + roll2 + roll3;
        if (unmodifiedTotal == 17 || unmodifiedTotal == 18)
        {
            isCriticalFailure = true;
        }
        else if (unmodifiedTotal == 3 || unmodifiedTotal == 4)
        {
            isCriticalSuccess = true;
        }

        return total;
    }
}