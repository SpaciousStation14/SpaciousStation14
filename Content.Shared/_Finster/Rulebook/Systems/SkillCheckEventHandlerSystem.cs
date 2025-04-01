using Content.Shared._Finster.Rulebook;
using Content.Shared._Finster.Rulebook.Events;

public sealed class SkillCheckEventHandlerSystem : EntitySystem
{
    [Dependency] private readonly SharedSkillCheckSystem _skillCheck = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AttributeCheckEvent>(OnAttributeCheck);
        SubscribeLocalEvent<SkillTypeCheckEvent>(OnSkillCheck);
    }

    private void OnAttributeCheck(ref AttributeCheckEvent ev)
    {
        if (ev.Handled)
            return;

        if (!TryComp<StatisticsComponent>(ev.User, out var stats))
        {
            ev.Handled = true;
            ev.Result = false;
            return;
        }

        var targetValue = stats.GetAttributeValue(ev.Attribute) + ev.ToolBonus;
        bool critSuccess, critFail;

        ev.Result = _skillCheck.TryRawCheck(
            ev.User,
            targetValue,
            ev.Bonus,
            out critSuccess,
            out critFail
        );

        ev.CriticalSuccess = critSuccess;
        ev.CriticalFailure = critFail;
        ev.Handled = true;
    }

    private void OnSkillCheck(ref SkillTypeCheckEvent ev)
    {
        if (ev.Handled)
            return;

        if (!TryComp<StatisticsComponent>(ev.User, out var stats))
        {
            ev.Handled = true;
            ev.Result = false;
            return;
        }

        var skillLevel = stats.GetSkillValue(ev.Skill);
        var attributeModifier = stats.GetModifier(
            stats.GetAttributeValue(ev.Skill.GetBaseAttribute())
        );
        var skillModifier = stats.GetSkillModifier(ev.Skill, skillLevel);
        var targetValue = 10 + attributeModifier + skillModifier + ev.ToolBonus;

        bool critSuccess, critFail;

        ev.Result = _skillCheck.TryRawCheck(
            ev.User,
            targetValue,
            0, // Bonus already factored into targetValue
            out critSuccess,
            out critFail
        );

        ev.CriticalSuccess = critSuccess;
        ev.CriticalFailure = critFail;
        ev.Handled = true;
    }
}