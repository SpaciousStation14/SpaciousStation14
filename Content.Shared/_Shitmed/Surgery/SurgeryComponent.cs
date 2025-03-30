using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Prototype("Surgeries")]
public sealed partial class SurgeryComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Priority;

    [DataField, AutoNetworkedField]
    public EntProtoId? Requirement;

    [DataField(required: true), AutoNetworkedField]
    public List<EntProtoId> Steps = new();

    [DataField("criticalSuccessHealBonus"), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier CriticalFailDamageSurgery = new()
    {
        DamageDict = new()
        {
            { "Blunt", 15f } // Negative values heal
        }
    };
}