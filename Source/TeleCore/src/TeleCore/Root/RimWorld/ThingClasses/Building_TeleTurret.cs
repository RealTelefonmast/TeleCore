﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Multiplayer.API;
using RimWorld;
using TeleCore.Systems.Events;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TeleCore;

/// <summary>
/// </summary>
public class Building_TeleTurret : Building_Turret, ITurretHolder, IFXLayerProvider
{
    private bool hasTurret;
    private bool canForceTargetDefault;

    //Comps
    protected CompPowerTrader powerComp;
    protected CompCanBeDormant dormantComp;
    protected CompInitiatable initiatableComp;
    protected CompMannable mannableComp;
    protected CompInteractable interactableComp;
    protected CompRefuelable refuelableComp;
    protected CompMechPowerCell powerCellComp;
    protected CompNetwork networkComp;

    private TurretGunSet turretSet;

    public TurretDefExtension Extension { get; private set; }

    //
    public override LocalTargetInfo CurrentTarget
    {
        get
        {
            if (!hasTurret) return LocalTargetInfo.Invalid;
            if (turretSet.KnownTargets.Count > 0)
                return turretSet.KnownTargets.First();
            return LocalTargetInfo.Invalid;
        }
    }

    public override Verb AttackVerb => turretSet.AttackVerb;
    public Verb_Tele TeleVerb => turretSet.AttackVerb as Verb_Tele;
    public TurretGun MainGun => turretSet.MainGun;

    public bool MannedByColonist => ManningPawn?.Faction == Faction.OfPlayer;
    public bool MannedByNonColonist => ManningPawn?.Faction != Faction.OfPlayer;
    public bool HoldingFire => turretSet.HoldingFire;

    //
    public Pawn ManningPawn => MannableComp?.ManningPawn;

    //
    public virtual LocalTargetInfo TargetOverride => forcedTarget;

    //TurretHolder
    public bool Active => (PowerTraderComp == null || PowerTraderComp.PowerOn) &&
                            (DormantComp == null || DormantComp.Awake) &&
                            (InitiatableComp == null || InitiatableComp.Initiated) &&
                            (this.interactableComp == null) &&
                            this.powerCellComp is not { depleted: true } &&
                            MannableComp is not { MannedNow: false }; //TODO: Change whether manning requires activating

    public bool PlayerControlled => Faction == Faction.OfPlayer || MannedByColonist;

    public TurretGunSet TurretSet => turretSet;
    public virtual Thing Caster => this;
    public virtual Thing HolderThing => this;
    public virtual CompPowerTrader PowerTraderComp => powerComp;
    public virtual CompCanBeDormant DormantComp => dormantComp;
    public virtual CompInitiatable InitiatableComp => initiatableComp;
    public virtual CompMannable MannableComp => mannableComp;
    public virtual CompRefuelable RefuelComp => refuelableComp;
    public virtual CompNetwork NetworkComp => networkComp;
    public virtual StunHandler Stunner => stunner;

    public virtual void Notify_OnProjectileFired()
    {

    }

    public new bool ThreatDisabled(IAttackTargetSearcher disabledFor)
    {
        if (!hasTurret) return true;
        if (base.ThreatDisabled(disabledFor)) return true;
        if (PowerTraderComp is { PowerOn: false }) return true;
        return MannableComp is { MannedNow: false };
    }

    public void Notify_LostTarget(LocalTargetInfo forcedTarget)
    {
        turretSet?.Notify_LostTarget(forcedTarget);
    }

    public void Notify_NewTarget(LocalTargetInfo currentTarget)
    {
        turretSet?.Notify_NewTarget(currentTarget);
    }

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Extension = def.TurretExtension();

        dormantComp = GetComp<CompCanBeDormant>();
        initiatableComp = GetComp<CompInitiatable>();
        powerComp = GetComp<CompPowerTrader>();
        mannableComp = GetComp<CompMannable>();
        interactableComp = GetComp<CompInteractable>();
        refuelableComp = GetComp<CompRefuelable>();
        powerCellComp = GetComp<CompMechPowerCell>();
        networkComp = GetComp<CompNetwork>();


        //Init Turrets
        if (Extension.HasTurrets)
        {
            hasTurret = true;
            turretSet = new TurretGunSet(Extension, this);
            canForceTargetDefault = Extension.turrets.Any(t => t.canForceTarget);
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (hasTurret)
        {
            turretSet.TickTurrets();
        }
    }

    /// <summary>
    /// This is a custom override to simple shoot at a target directly, no cooldown or other settings.
    /// </summary>
    public void DoAttackNow(LocalTargetInfo targ, int turretIndex = -1)
    {
        turretSet.DoAttackNow(targ, turretIndex);
    }

    /// <summary>
    /// Should be called to fix the turret's rotation to the current target.
    /// </summary>
    public void ClearAttack()
    {
        turretSet.ClearAttack();
    }

    //Basic Turret Functions
    //[SyncMethod]
    public sealed override void OrderAttack(LocalTargetInfo targ)
    {
        turretSet?.TryOrderAttack(targ);
    }

    //[SyncMethod]
    public void ResetOrderedAttack()
    {
        if (!hasTurret) return;
        forcedTarget = LocalTargetInfo.Invalid;
        turretSet.ResetOrderedAttack();
        OnResetOrderedAttack();
    }

    protected virtual void OnOrderAttack(LocalTargetInfo targ)
    {
    }

    protected virtual void OnResetOrderedAttack()
    {
    }

    //
    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (!hasTurret) return;
        turretSet.Draw(drawLoc);
        TeleVerb?.DrawVerb(drawLoc);
    }

    private static StringBuilder sb = new StringBuilder();

    public override string GetInspectString()
    {
        sb.Clear();
        var inspectString = base.GetInspectString();
        if (!inspectString.NullOrEmpty())
            sb.AppendLine(inspectString);

        if (hasTurret)
            sb.AppendFormat(turretSet.InspectString());
        return sb.ToString().TrimEndNewlines();
    }

    //
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
            yield return gizmo;

        if (!hasTurret) yield break;

        foreach (var turretGizmo in turretSet.TurretGizmos())
            yield return turretGizmo;

        if (canForceTargetDefault)
        {
            var command_VerbTarget = new Command_VerbTarget();
            command_VerbTarget.defaultLabel = "CommandSetForceAttackTarget".Translate();
            command_VerbTarget.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
            command_VerbTarget.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
            command_VerbTarget.verb = AttackVerb;
            command_VerbTarget.hotKey = KeyBindingDefOf.Misc4;
            command_VerbTarget.drawRadius = false;
            command_VerbTarget.requiresAvailableVerb = false;
            command_VerbTarget.disabled = !Active;
            if (Spawned && Position.Roofed(Map)) //TODO? this.IsMortarOrProjectileFliesOverhead
                command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            yield return command_VerbTarget;

            if (TargetOverride.IsValid)
            {
                var command_Action = new Command_Action
                {
                    defaultLabel = "CommandStopForceAttack".Translate(),
                    defaultDesc = "CommandStopForceAttackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt"),
                    action = delegate
                    {
                        ResetOrderedAttack();
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                };
                if (!TargetOverride.IsValid)
                    command_Action.Disable("CommandStopAttackFailNotForceAttacking".Translate());
                command_Action.hotKey = KeyBindingDefOf.Misc5;
                yield return command_Action;
            }

            /*
            if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
            {
                command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            }
            */
        }

        /*
        IEnumerator<Gizmo> enumerator = null;
        CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
        if (compChangeableProjectile != null)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandExtractShell".Translate();
            command_Action.defaultDesc = "CommandExtractShellDesc".Translate();
            command_Action.icon = ContentFinder<Texture2D>.Get("Rimatomics/Things/Resources/sabot/sabot_c", true);
            command_Action.alsoClickIfOtherInGroupClicked = false;
            command_Action.action = new Action(this.dumpShells);
            if (compChangeableProjectile.Projectile == null)
            {
                command_Action.Disable("NoSabotToExtract".Translate());
            }
            yield return command_Action;
        }
        */
    }

    #region FX Implementation

    //Basics
    public virtual bool FX_ProvidesForLayer(FXArgs args)
    {
        if (args.layerTag == "FXTurret")
            return true;
        return false;
    }

    public virtual CompPowerTrader FX_PowerProviderFor(FXArgs args)
    {
        return null!;
    }

    //Layer
    public virtual bool? FX_ShouldDraw(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetOpacity(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetRotation(FXLayerArgs args)
    {
        return args.index switch
        {
            2 => MainGun?.TurretRotation,
            _ => null
        };
    }

    public virtual float? FX_GetRotationSpeedOverride(FXLayerArgs args)
    {
        return null;
    }

    public virtual float? FX_GetAnimationSpeedFactor(FXLayerArgs args)
    {
        return null;
    }

    public virtual int? FX_SelectedGraphicIndex(FXLayerArgs args)
    {
        return null;
    }

    public virtual Color? FX_GetColor(FXLayerArgs args)
    {
        return null;
    }

    public virtual Vector3? FX_GetDrawPosition(FXLayerArgs args)
    {
        return null;
    }

    public virtual Func<RoutedDrawArgs, bool> FX_GetDrawFunc(FXLayerArgs args)
    {
        return null!;
    }

    //Effecters
    public virtual bool? FX_ShouldThrowEffects(FXEffecterArgs args)
    {
        return true;
    }

    public virtual TargetInfo FX_Effecter_TargetAOverride(FXEffecterArgs args)
    {
        return null;
    }

    public virtual TargetInfo FX_Effecter_TargetBOverride(FXEffecterArgs args)
    {
        return null;
    }

    public virtual void FX_OnEffectSpawned(FXEffecterSpawnedEventArgs args)
    {
    }

    #endregion
}