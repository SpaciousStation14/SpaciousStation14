using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Drugs;
using Content.Shared.Drunk;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Interaction;
using Content.Shared.Medical;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared._Finster.Rulebook;
using Content.Shared._Finster.Rulebook.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Medical;
/// <summary>
///     This stores the eye exam system for <see cref="PenLightComponent"/>
/// </summary>
public sealed class PenLightSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PenLightComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PenLightComponent, PenLightDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, PenLightComponent component, ref AfterInteractEvent args)
    {
        if (args.Handled
            || args.Target is not {} target
            || target == null
            || !args.CanReach
            || !HasComp<MobStateComponent>(target)
            || !_powerCell.HasDrawCharge(uid, user: args.User))
            return;
        args.Handled = TryStartExam(uid, target, args.User, component);
    }

    private void OnDoAfter(Entity<PenLightComponent> uid, ref PenLightDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Target == null
            || !_powerCell.HasDrawCharge(uid, user: args.User))
            return;

        var ev = new SkillTypeCheckEvent(
            User: args.User,
            Skill: SkillType.Diagnosis,
            SituationalBonus: 0,
            ToolBonus: 0,
            CriticalSuccess: false,
            CriticalFailure: false
        );

        RaiseLocalEvent(ref ev);

        // Handles skill check results
        if (!ev.Result)
        {
            if (ev.CriticalFailure)
            {
                _popup.PopupEntity(Loc.GetString("penlight-critical-failure"), args.User, args.User);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("penlight-failure"), args.User, args.User);
            }
            return;
        }

        if (ev.CriticalSuccess)
        {
            _popup.PopupEntity(Loc.GetString("penlight-critical-success"), args.User, args.User);
        }

        OpenUserInterface(args.User, uid);
        Diagnose(uid, args.Target.Value);
        args.Handled = true;
    }

    /// <summary>
    ///     Checks if the PointLight component is enabled.
    /// </summary>
    private bool IsLightEnabled(EntityUid uid)
    {
        return TryComp<PointLightComponent>(uid, out var pointLight) && pointLight.Enabled;
    }

    /// <summary>
    ///     Actually handles the exam interaction.
    /// </summary>
    public bool TryStartExam(EntityUid uid, EntityUid target, EntityUid user, PenLightComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!IsLightEnabled(uid))
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("penlight-off"), uid, user);
            return false;
        }
        // can't examine your own eyes, dingus
        if (user == target)
        {
            _popup.PopupEntity(Loc.GetString("penlight-cannot-examine-self"), uid, user);
            return false;
        }

        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, component.ExamSpeed, new PenLightDoAfterEvent(),
            uid, target, uid)
        {
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            NeedHand = true
        });
    }
    private void OpenUserInterface(EntityUid user, EntityUid penlight)
    {
        if (!_uiSystem.HasUi(penlight, PenLightUiKey.Key))
            return;

        _uiSystem.OpenUi(penlight, PenLightUiKey.Key, user);
    }

    /// <summary>
    ///     Runs the checks for the different types of eye damage
    /// </summary>
    private void Diagnose(EntityUid penlight, EntityUid target)
    {
        if (!_uiSystem.HasUi(penlight, PenLightUiKey.Key)
            || !HasComp<EyeComponent>(target)
            || !HasComp<DamageableComponent>(target))
            return;

        // Blind
        var blind = _entityManager.HasComponent<PermanentBlindnessComponent>(target);

        // Drunk
        var drunk = _entityManager.HasComponent<DrunkComponent>(target);

        // EyeDamage
        var eyeDamage = false;
        if (TryComp<BlindableComponent>(target, out var eyeDam))
        {
            eyeDamage = eyeDam.EyeDamage > 0 && eyeDam.EyeDamage < 6; //6 means perma-blind
        }

        // Hallucinating
        var seeingRainbows = _entityManager.HasComponent<SeeingRainbowsComponent>(target);

        // Healthy
        var healthy = !(blind || drunk || eyeDamage || seeingRainbows);

        _uiSystem.ServerSendUiMessage(
            penlight,
            PenLightUiKey.Key,
            new PenLightUserMessage(GetNetEntity(target),
                blind,
                drunk,
                eyeDamage,
                healthy,
                seeingRainbows
            )
        );
    }
}
