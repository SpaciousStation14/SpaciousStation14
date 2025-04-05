// Content.Shared._Finster.Rulebook/SkillExtensions.cs
using Content.Shared._Finster.Rulebook;

public static class SkillExtensions
{
    public static SkillDifficulty GetDifficulty(this SkillType skill)
        => skill switch
        {
            SkillType.FirstAid => SkillDifficulty.Easy,
            SkillType.Diagnosis => SkillDifficulty.Average,
            _ => SkillDifficulty.Average // Default case
        };

    public static AttributeType GetBaseAttribute(this SkillType skill)
        => skill switch
        {
            SkillType.FirstAid => AttributeType.Intelligence,
            SkillType.Diagnosis => AttributeType.Intelligence,
            _ => AttributeType.Intelligence // Default case
        };
}