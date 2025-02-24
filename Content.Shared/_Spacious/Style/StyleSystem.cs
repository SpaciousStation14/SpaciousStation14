using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Explosion;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._Spacious.Style;

public sealed class StyleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MobStyleComponent, ComponentInit>(OnMobStyleInit);

        SubscribeLocalEvent<MobStyleComponent, ClothingDidEquippedEvent>(ClothingDidEquippedHandler);
        SubscribeLocalEvent<MobStyleComponent, ClothingDidUnequippedEvent>(ClothingDidUnequippedHandler);
        SubscribeLocalEvent<MobStyleComponent, ProjectileReflectAttemptEvent>(ProjectileReflectAttemptHandler);
        SubscribeLocalEvent<MobStyleComponent, HitScanReflectAttemptEvent>(HitscanReflectAttemptHandler);
        SubscribeLocalEvent<MobStyleComponent, GetExplosionResistanceEvent>(GetExplosionResistanceHandler);
    }

    private void OnMobStyleInit(EntityUid uid, MobStyleComponent comp, ComponentInit args)
    {
        UpdateMobStyleCapGain(comp);
    }

    private void ClothingDidEquippedHandler(EntityUid uid, MobStyleComponent comp, ClothingDidEquippedEvent args)
    {
        if (!TryComp<ItemStyleComponent>(args.Clothing, out var stylishComp))
            return;
        UpdateMobStyleCapGain(comp);
    }

    private void ClothingDidUnequippedHandler(EntityUid uid, MobStyleComponent comp, ClothingDidUnequippedEvent args)
    {
        if (!TryComp<ItemStyleComponent>(args.Clothing, out var stylishComp))
            return;
        UpdateMobStyleCapGain(comp);
    }

    // These events are only fired server-side.
    // Here's to hoping they will be predicted one day.
    private void HitscanReflectAttemptHandler(EntityUid uid, MobStyleComponent comp, ref HitScanReflectAttemptEvent args)
    {
        var mul = 1f;
        if (TryComp<GunComponent>(args.SourceItem, out var gun))
            mul = gun.StyleCostMultiplier;
        args.Reflected = TryDodge(uid, comp, args.Prototype.Damage?.GetTotalPositive() ?? 0, mul);
    }

    private void ProjectileReflectAttemptHandler(EntityUid uid, MobStyleComponent comp, ref ProjectileReflectAttemptEvent args)
    {
        if (!TryComp<ProjectileComponent>(args.ProjUid, out var projComp))
            return; // wtf?

        args.Cancelled = TryDodge(uid, comp, projComp.Damage.GetTotalPositive(), projComp.StyleCostMultiplier);
    }

    private bool TryDodge(EntityUid uid, MobStyleComponent comp, FixedPoint2 totalDamage, float dodgeCostMultiplier = 1f, bool allowOverdraft = true)
    {
        var styleConsumed = (float) totalDamage * comp.DodgeCostProjectile * dodgeCostMultiplier;

        UpdateMobStyle(comp);

        // Dodges are disabled until style regenerates above zero.
        if (comp.CurrentStyle <= 0 || comp.StyleCap == 0)
            return false; // ain't slick enuff pal'

        // Allows to "overdraft" style points by going into negatives.
        if (allowOverdraft && styleConsumed > comp.CurrentStyle + comp.StyleCap * 3 ||
            !allowOverdraft && styleConsumed > comp.CurrentStyle)
            return false; // ain't slick enuff pal'

        comp.CurrentStyle -= styleConsumed;
        _popup.PopupEntity(Loc.GetString("style-dodged-message"), uid);
        _audio.PlayPvs(comp.DodgeSound, uid);
        return true;
    }

    private void GetExplosionResistanceHandler(EntityUid uid, MobStyleComponent comp, ref GetExplosionResistanceEvent args)
    {
        if (!args.IsReal)
            return;
        UpdateMobStyle(comp);

        if (comp.CurrentStyle <= 0)
            return;

        FixedPoint2 totalDamage = args.Damage.GetTotalPositive();

        float styleConsumed = (float) totalDamage * comp.DodgeCostExplosion;

        var coeff = MathF.Min(1, comp.CurrentStyle / styleConsumed);
        totalDamage *= coeff;
        // overdraft still allowed in case the floating point error somehow fuck us over
        DebugTools.Assert(TryDodge(uid, comp, totalDamage, 1), "Explosion dodge failed even though it was supposed to succeed.");
        args.DamageCoefficient = 1 - coeff;
    }

    public bool UpdateSlickness(EntityUid mobUid, MobStyleComponent? comp = null)
    {
        if (!Resolve(mobUid, ref comp))
            return false;
        UpdateMobStyle(comp);
        return true;
    }

    // This was generated by deepseek after i fed it my schizocode.
    // The worst thing is, it's more readable than what i've wrote.
    // I am genuinely fucking upset.
    public void UpdateMobStyle(MobStyleComponent comp)
    {
        if (comp.LastStyleUpdate == _timing.CurTime)
            return;

        Dirty(comp.Owner, comp);

        float totalTime = (float) (_timing.CurTime - comp.LastStyleUpdate).TotalSeconds;
        comp.LastStyleUpdate = _timing.CurTime;

        if (comp.CurrentStyle < 0)
        {
            float regenRate = MathF.Max(comp.BackupInnateStyleGain, comp.StyleGain); // deepseek fuckup 1
            float backupRegen = regenRate * totalTime;
            float newStyle = comp.CurrentStyle + backupRegen;

            if (newStyle >= 0 && comp.BackupInnateStyleGain > comp.StyleGain) // fuckup 2
            {
                // Calculate time needed to reach zero and apply remaining time to normal gain
                float timeToZero = (-comp.CurrentStyle) / comp.BackupInnateStyleGain;
                float remainingTime = totalTime - timeToZero;

                comp.CurrentStyle = 0;
                // Apply normal gain for the remaining time
                float normalGain = comp.StyleGain * remainingTime;
                // fuckup 3
                comp.CurrentStyle = MathF.Max(0, MathF.Min(comp.CurrentStyle + normalGain, comp.StyleCap));
            }
            else
            {
                // Still negative; update normally
                comp.CurrentStyle = newStyle;
            }
        }
        else
        {
            // Handle normal regeneration or degeneration
            float gained = comp.StyleGain * totalTime;

            if (comp.StyleGain < 0)
            {
                // Losing style: clamp to 0
                comp.CurrentStyle = MathF.Max(0, comp.CurrentStyle + gained);
            }
            else
            {
                // Gaining style: clamp to cap
                comp.CurrentStyle = MathF.Min(comp.CurrentStyle + gained, comp.StyleCap);
            }
        }
    }



    public bool UpdateMobStyleCapGain(EntityUid uid, MobStyleComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;
        UpdateMobStyleCapGain(comp);
        return true;
    }

    public void UpdateMobStyleCapGain(MobStyleComponent comp)
    {
        UpdateMobStyle(comp); // Update style before changing cap and gain values
        comp.StyleCap = comp.InnateStyleCap;
        comp.StyleGain = comp.InnateStyleGain;

        if (!TryComp<InventoryComponent>(comp.Owner, out var inventory))
            return;

        var slotenum = _inventory.GetSlotEnumerator(comp.Owner);

        while (slotenum.MoveNext(out var slot))
        {
            if (!TryComp<ClothingComponent>(slot.ContainedEntity, out var clothingComp) ||
                !TryComp<ItemStyleComponent>(slot.ContainedEntity, out var itemStyle) || 
                !_inventory.TryGetContainingSlot(slot.ContainedEntity.Value, out var slotdef) ||
                (slotdef.SlotFlags & clothingComp.Slots) == 0 || // for clothing being somehow stuffed into pockets
                (slotdef.SlotFlags & itemStyle.Slots) == 0)      // for revolvers being stuffed into suit storage
                continue;

            comp.StyleCap = MathF.Max(comp.StyleCap + itemStyle.StyleCapIncrease, 0);
            comp.StyleGain += itemStyle.StyleGainIncrease;
        }
        UpdateMobStyle(comp); // update style once again to make sure we don't go over cap.
                              // Technically not needed(?) since it will be called just before the style is used.
    }

    /// <summary>
    /// Adds or subtracts style. Will not go above style cap.
    /// </summary>
    /// <returns>True if successful, false if the entity doesn't have MobStyleComponent.</returns>
    public bool AdjustStyle(EntityUid uid, float styleChange, MobStyleComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;
        AdjustStyle(comp, styleChange);
        return true;
    }

    /// <summary>
    /// Adds or subtracts style. Will not go above style cap.
    /// </summary>
    public void AdjustStyle(MobStyleComponent comp, float styleChange)
    {
        comp.CurrentStyle += styleChange;
        UpdateMobStyle(comp);
    }

    /// <summary>
    /// Sets the new style. Will not go above style cap.
    /// </summary>
    /// <returns>True if successful, false if the entity doesn't have MobStyleComponent.</returns>
    public bool SetStyle(EntityUid uid, float newStyle, MobStyleComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;
        AdjustStyle(comp, newStyle);
        return true;
    }

    /// <summary>
    /// Sets the new style. Will not go above style cap.
    /// </summary>
    public void SetStyle(MobStyleComponent comp, float newStyle)
    {
        comp.CurrentStyle = newStyle;
        UpdateMobStyle(comp);
    }

#if DEBUG

    // Copypaste of UpdateMobStyleDebug, for showing style in real time (in VV) without actually updating it every frame.
    // For better utility, go to ViewVariablesInstanceObject.cs, line 109 and change the 500 delay to 1.
    public float UpdateMobStyleDebug(MobStyleComponent comp)
    {
        float CurrentStyle = comp.CurrentStyle;
        float totalTime = (float) (_timing.CurTime - comp.LastStyleUpdate).TotalSeconds;

        if (CurrentStyle < 0)
        {
            float regenRate = MathF.Max(comp.BackupInnateStyleGain, comp.StyleGain); // deepseek fuckup 1
            float backupRegen = regenRate * totalTime;
            float newStyle = CurrentStyle + backupRegen;

            if (newStyle >= 0 && comp.BackupInnateStyleGain > comp.StyleGain) // fuckup 2
            {
                // Calculate time needed to reach zero and apply remaining time to normal gain
                float timeToZero = (-CurrentStyle) / comp.BackupInnateStyleGain;
                float remainingTime = totalTime - timeToZero;

                CurrentStyle = 0;
                // Apply normal gain for the remaining time
                float normalGain = comp.StyleGain * remainingTime;
                // fuckup 3, wasn't clamped between 0 and comp.StyleCap
                CurrentStyle = MathF.Max(0, MathF.Min(CurrentStyle + normalGain, comp.StyleCap));

            }
            else
            {
                // Still negative; update normally
                CurrentStyle = newStyle;
            }
        }
        else
        {
            // Handle normal regeneration or degeneration
            float gained = comp.StyleGain * totalTime;

            if (comp.StyleGain < 0)
            {
                // Losing style: clamp to 0
                CurrentStyle = MathF.Max(0, CurrentStyle + gained);
            }
            else
            {
                // Gaining style: clamp to cap
                CurrentStyle = MathF.Min(CurrentStyle + gained, comp.StyleCap);
            }
        }
        return CurrentStyle;
    }
#endif

}

