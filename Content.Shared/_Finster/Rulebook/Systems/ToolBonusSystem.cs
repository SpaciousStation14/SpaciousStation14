using Content.Shared._Finster.Rulebook;
using Content.Shared._Finster.Rulebook.Events;
using Content.Shared.Hands.Components;

public sealed class ToolBonusSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SkillTypeCheckEvent>(OnSkillCheck,
            before: new[] { typeof(SkillCheckEventHandlerSystem) });
    }

    private void OnSkillCheck(ref SkillTypeCheckEvent ev)
    {
        if (ev.Handled)
            return;

        // Check for tools in user's hands
        if (TryComp<HandsComponent>(ev.User, out var hands))
        {
            foreach (var hand in hands.Hands.Values)
            {
                if (hand.HeldEntity is not { } tool)
                    continue;

                // Add tool-specific bonuses
                if (TryComp<ToolQualityComponent>(tool, out var toolQuality))
                {
                    ev.ToolBonus += GetToolBonusForSkill(toolQuality, ev.Skill);
                }
            }
        }
    }

    private int GetToolBonusForSkill(ToolQualityComponent tool, SkillType skill)
    {
        return skill switch
        {
            SkillType.FirstAid when tool.Qualities.Contains("Idiotproof") => 8,
            SkillType.FirstAid when tool.Qualities.Contains("EasyTopical") => 2,
            SkillType.FirstAid when tool.Qualities.Contains("AdvancedTopical") => -2,
            _ => 0
        };
    }
}