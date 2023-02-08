﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TeleCore.Static.Utilities;
using UnityEngine;
using Verse;

namespace TeleCore
{
    //TODO: Add leaking functionality, broken transmitters losing values
    public class Comp_NetworkStructure : ThingComp, INetworkStructure, IFXHolder
    {
        //
        private NetworkMapInfo networkInfo;

        private List<NetworkSubPart> networkParts;
        private Dictionary<NetworkDef, NetworkSubPart> networkPartByDef;
        private NetworkCellIO cellIO;

        //Debug
        protected static bool DebugConnectionCells = false;
        private IFXHolder ifxHolderImplementation;

        //
        public NetworkSubPart this[NetworkDef def] => networkPartByDef.TryGetValue(def, out var value) ? value : null;

        //
        public CompProperties_NetworkStructure Props => (CompProperties_NetworkStructure)base.props;
        public CompPowerTrader CompPower { get; private set; }
        public CompFlickable CompFlick { get; private set; }
        public CompFX CompFX { get; private set; }

        //
        public Thing Thing => parent;
        public List<NetworkSubPart> NetworkParts => networkParts;
        public NetworkCellIO GeneralIO => cellIO;

        public bool IsPowered => CompPower?.PowerOn ?? true;

        //
        public virtual bool IsActiveOverride => true;

        #region FX Implementation

        public virtual bool FX_ProvidesForLayer(FXLayerArgs args)
        {
            return args.layerTag switch
            {
                FXLayerData._NetworkHolderTag => true,
                _ => false,
            };
        }

        public virtual CompPowerTrader FX_PowerProviderFor(FXLayerArgs args) => null;

        public virtual bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                1 => networkParts.Any(t => t?.HasConnection ?? false),
                _ => true
            };
        }

        public virtual float? FX_GetOpacity(FXLayerArgs args) => 1f;

        public virtual float? FX_GetRotation(FXLayerArgs args) => null;
        public virtual float? FX_GetAnimationSpeedFactor(FXLayerArgs args) => null;
        
        public virtual Color? FX_GetColor(FXLayerArgs args)         
        {
            return args.index switch
            {
                0 => networkParts[0].Container.Color,
                _ => Color.white
            };
        }
        
        public virtual Vector3? FX_GetDrawPosition(FXLayerArgs args)  
        {
            return parent.DrawPos;
        }
        
        public virtual Action<FXLayer> FX_GetAction(FXLayerArgs args) => null!;
        
        public virtual bool? FX_ShouldThrowEffects(FXLayerArgs args) => true;
        public void FX_OnEffectSpawned(EffecterEffectSpawnedArgs effectSpawnedArgs)
        {
        }
        
        #endregion
        
        //SaveData
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref networkParts, "networkParts", LookMode.Deep, this);

            //
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (networkParts.NullOrEmpty())
                {
                    TLog.Warning($"Could not load network parts for {parent}... Correcting.");
                }
            }
        }

        //Init Construction
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            //
            base.PostSpawnSetup(respawningAfterLoad);

            //
            CompPower = parent.TryGetComp<CompPowerTrader>();
            CompFlick = parent.TryGetComp<CompFlickable>();
            CompFX = parent.TryGetComp<CompFX>();

            //
            cellIO = new NetworkCellIO(Props.generalIOPattern, parent);
            networkInfo = parent.Map.TeleCore().NetworkInfo;

            //Create NetworkComponents
            if (respawningAfterLoad && (networkParts.Count != Props.networks.Count))
            {
                TLog.Warning($"Spawning {parent} after load with missing parts... Correcting.");
            }
            
            //
            if(!respawningAfterLoad)
                networkParts = new List<NetworkSubPart>(Math.Max(1, Props.networks.Count));
            
            networkPartByDef = new Dictionary<NetworkDef, NetworkSubPart>(Props.networks.Count);
            for (var i = 0; i < Props.networks.Count; i++)
            {
                var compProps = Props.networks[i];
                NetworkSubPart subPart = null;
                if (!networkParts.Any(p => p.NetworkDef == compProps.networkDef))
                {
                    subPart = (NetworkSubPart) Activator.CreateInstance(compProps.workerType, args: new object[] {this, compProps});
                    networkParts.Add(subPart);
                }

                if (subPart == null)
                    subPart = networkParts[i];

                networkPartByDef.Add(compProps.networkDef, subPart);
                subPart.SubPartSetup(respawningAfterLoad);
            }
            
            //Check for neighbor intersections
            //Regen network after all data is set
            networkInfo.Notify_NewNetworkStructureSpawned(this);
        }
        
        //Deconstruction
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            //Regen network after all data is set
            networkInfo.Notify_NetworkStructureDespawned(this);

            foreach (var networkPart in NetworkParts)
            {
                networkPart.PostDestroy(mode, previousMap);
            }
        }

        //
        public virtual bool CanInteractWith(INetworkSubPart interactor, INetworkSubPart otherPart)
        {
            return true;
        }
        
        public virtual void NetworkPostTick(NetworkSubPart networkSubPart, bool isPowered)
        {

        }

        public virtual void NetworkPartProcessorTick(INetworkSubPart netPart)
        {
        }

        public virtual void Notify_ReceivedValue()
        {
        }

        //
        public void Notify_StructureAdded(INetworkStructure other)
        {
            //structureSet.AddNewStructure(other);
        }

        public void Notify_StructureRemoved(INetworkStructure other)
        {
            //structureSet.RemoveStructure(other);
        }

        //
        public virtual bool AcceptsValue(NetworkValueDef value)
        {
            return true;
        }
        
        public virtual bool CanConnectToOther(INetworkStructure other)
        {
            return true;
        }
        
        //UI
        public override void PostDraw()
        {
            base.PostDraw();

            //   
            foreach (var networkPart in NetworkParts)
            {
                networkPart.Draw();
                if (DebugConnectionCells && Find.Selector.IsSelected(parent))
                {
                    GenDraw.DrawFieldEdges(networkPart.CellIO.OuterConnnectionCells.ToList(), Color.cyan);
                    GenDraw.DrawFieldEdges(networkPart.CellIO.InnerConnnectionCells.ToList(), Color.green);
                }
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);

            //
            foreach (var networkPart in NetworkParts)
            {
                networkPart.NetworkDef.TransmitterGraphic?.Print(layer, Thing, 0, networkPart);
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var networkSubPart in NetworkParts)
            {
                sb.AppendLine(networkSubPart.NetworkInspectString());
            }

            /*TODO: ADD THIS TO COMPONENT DESC
            if (!Network.IsWorking)
                sb.AppendLine("TR_MissingNetworkController".Translate());
            //TODO: Make reasons for multi roles
            if (!Network.ValidFor(Props.NetworkRole, out string reason))
            {
                sb.AppendLine("TR_MissingConnection".Translate() + ":");
                if (!reason.NullOrEmpty())
                {
                    sb.AppendLine("   - " + reason.Translate());
                }
            }
            */

            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            /*
            foreach (var gizmo in networkParts.Select(c => c.SpecialNetworkDescription))
            {
            yield return gizmo;
            }
            */

            foreach (var networkPart in NetworkParts)
            {
                foreach (var partGizmo in networkPart.GetPartGizmos())
                {
                    yield return partGizmo;
                }
            }

            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (!DebugSettings.godMode) yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Draw Networks",
                action = delegate
                {
                    foreach (var networkPart in NetworkParts)
                    {
                        networkInfo[networkPart.NetworkDef].ToggleShowNetworks();
                    }
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Draw Connections",
                action = delegate { DebugConnectionCells = !DebugConnectionCells; }
            };

            yield return new Command_Action
            {
                defaultLabel = "Set Node Dirty",
                action = delegate
                {
                    NetworkParts[0].Network.Graph.Notify_StateChanged(NetworkParts[0]);
                }
            };

            yield return new Command_Target
            {
                defaultLabel = "Get Path",
                targetingParams = TargetingParameters.ForBuilding(),
                action = delegate (LocalTargetInfo target) {
                    if (target.Thing is not ThingWithComps compThing) return;
                    var netComp = compThing.TryGetComp<Comp_NetworkStructure>();
                    var part = netComp[NetworkParts[0].NetworkDef];
                    if (part == null) return;

                    var path = part.Network.Graph.ProcessRequest(new NetworkGraphPathRequest(networkParts[0], part));
                    if (!path.IsValid) return;
                    for (var i = 0; i < path.allPaths.Length; i++)
                    {
                        var graphPath = path.allPaths[i];
                        TLog.Debug($"[{i}] {graphPath}");
                    }
                }
            };
        }
    }
}
