using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._Spacious.Style;

/// <summary>
/// 
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MobStyleComponent : Component
{
    [DataField("styleCap"), AutoNetworkedField]
    public float InnateStyleCap = 0f;

    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField("styleGain"), AutoNetworkedField]
    public float InnateStyleGain = 0f;

    /// <summary>
    /// Backup regeneration rate is used in case a mob:
    /// a) has negative or zero regeneration rate and has less than zero style;
    /// b) has negative style and its normal regen rate is less than backup rate
    /// Otherwise, normal rate is used.
    /// </summary>
    /// <remarks>
    /// Per second.
    /// </remarks>
    [DataField("styleGainBackup"), AutoNetworkedField]
    public float BackupInnateStyleGain = 0f;


    /// <summary>
    /// How much style is consumed every time the user dodges a projectile/hitscan.
    /// Acts as a multiplier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DodgeCostProjectile = 1f;

    /// <summary>
    /// How much style is consumed every time the user dodges an explosion.
    /// Acts as a multiplier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DodgeCostExplosion = 1f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier DodgeSound = new SoundPathSpecifier("/Audio/_Spacious/dodge.ogg");

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float StyleCap;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float StyleGain;

    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float CurrentStyle = 0f;

#if DEBUG
    [ViewVariables(VVAccess.ReadOnly)]
    public float DebugCurrentStyleRealtime
    {
        get
        {
            StyleSystem? sys = null;
            IoCManager.Resolve<IEntitySystemManager>().Resolve<StyleSystem>(ref sys);
            return sys.UpdateMobStyleDebug(this);

        }
    }
#endif

    /// <summary>
    /// This tracks when was the last time slickness was consumed.
    /// Same principle as with GunSystem's LastFired, this should
    /// help avoid mispredictions. I think.
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastStyleUpdate;
}

