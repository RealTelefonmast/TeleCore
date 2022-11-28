﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class PipeNetwork : IExposable
    {
        protected NetworkDef def;
        protected NetworkRank networkRank = NetworkRank.Alpha;

        protected PipeNetworkManager parentManager;

        protected NetworkPartSet partSet;
        protected NetworkContainerSet containerSet;

        public PipeNetworkManager ParentManager => parentManager;

        public NetworkGraph Graph { get; internal set; }
        public NetworkPartSet PartSet => partSet;
        public NetworkContainerSet ContainerSet => containerSet;

        public INetworkStructure NetworkController => PartSet.Controller?.Parent;
        public List<IntVec3> NetworkCells { get; }

        public NetworkDef Def => def;
        public NetworkRank NetworkRank => networkRank;
        public int ID { get; set; } = -1;

        public bool IsValid => parentManager.AllNetworks.Contains(this);
        public bool HasGraph => Graph != null && Graph.AllNodes.Count > 1;
        
        public virtual bool IsWorking => !def.UsesController || (NetworkController?.IsPowered ?? false);
        public virtual float TotalNetworkValue => ContainerSet.TotalNetworkValue;
        public virtual float TotalStorageNetworkValue => ContainerSet.TotalStorageValue;

        //DEBUG
        internal bool DrawInternalGraph = false;
        internal bool DrawGraphCachedResults = false;
        internal bool DrawAdjacencyList = false;

        public PipeNetwork(NetworkDef def, PipeNetworkManager manager)
        {
            this.ID = PipeNetworkManager.MasterID++;
            this.def = def;
            parentManager = manager;
            partSet = new NetworkPartSet(def, null);
            containerSet = new NetworkContainerSet();
            NetworkCells = new List<IntVec3>();
        }

        public virtual void ExposeData()
        {

        }

        public virtual void Tick()
        {
            foreach (var part in PartSet.FullSet)
            {
                part.NetworkTick();
            }
        }

        public virtual void Draw()
        {
            if(DrawGraphCachedResults)
                Graph.Debug_DrawCachedResults();
            Graph.Debug_DrawPressure();
            
            //
            var selThing = Find.Selector.SingleSelectedThing;
            if (selThing != null && selThing.TryGetComp(out Comp_NetworkStructure comp))
            {
                foreach (var compNetworkPart in comp.NetworkParts)
                {  
                    if (DrawAdjacencyList)
                        GenDraw.DrawFieldEdges(Graph.AdjacencyLists[compNetworkPart].Select(c => c.Parent.Thing.Position).ToList(), Color.red);  
                    compNetworkPart.CellIO.DrawIO();
                }
            }
        }

        public void DrawOnGUI()
        {
            Graph.Debug_DrawOverlays();
            if(DrawInternalGraph)
                Graph.Debug_DrawGraphOnUI();
        }

        //
        public float TotalValueFor(NetworkValueDef valueDef)
        {
            return ContainerSet.GetValueByType(valueDef);
        }

        public float TotalValueFor(NetworkRole ofRole)
        {
            return ContainerSet.GetTotalValueByRole(ofRole);
        }

        public float TotalValueFor(NetworkValueDef valueDef, NetworkRole ofRole)
        {
            return ContainerSet.GetValueByTypeByRole(valueDef, ofRole);
        }

        public bool ValidFor(NetworkRole role, out string reason)
        {
            reason = string.Empty;
            NetworkRole[] values = (NetworkRole[])Enum.GetValues(typeof(NetworkRole));
            foreach (var value in values)
            {
                if ((role & value) == value)
                {
                    switch (value)
                    {
                        case NetworkRole.Consumer:
                            reason = "TR_ConsumerLack";
                            return PartSet.FullSet.Any(x => x.NetworkRole.HasFlag(NetworkRole.Storage) || x.NetworkRole.HasFlag(NetworkRole.Producer));
                        case NetworkRole.Producer:
                            reason = "TR_ProducerLack";
                            return PartSet.FullSet.Any(x => x.NetworkRole.HasFlag(NetworkRole.Storage) || x.NetworkRole.HasFlag(NetworkRole.Consumer));
                        case NetworkRole.Transmitter:
                            break;
                        case NetworkRole.Storage:
                            break;
                        case NetworkRole.Controller:
                            break;
                        case NetworkRole.AllContainers:
                            break;
                        default: break;
                    }
                }
            }
            return true;
        }

        //
        public bool TryGetNodePath(NetworkSubPart networkSubPart, NetworkRole storage)
        {
            
            return false;
        }

        //Data Updates
        public void Notify_AddPart(INetworkSubPart part)
        {
            ParentManager.Notify_AddPart(part);

            //
            if (PartSet.AddNewComponent(part))
                NetworkCells.AddRange(part.CellIO.InnerConnnectionCells);

            ContainerSet.AddNewContainerFrom(part);
        }

        public void Notify_RemovePart(INetworkSubPart part)
        {
            ParentManager.Notify_RemovePart(part);

            //
            PartSet.RemoveComponent(part);
            containerSet.RemoveContainerFrom(part);
            foreach (var cell in part.CellIO.InnerConnnectionCells)
            {
                NetworkCells.Remove(cell);
            }
        }

        public override string ToString()
        {
            return $"{def}[{ID}][{networkRank}]";
        }

        public string GreekLetter
        {
            get
            {
                switch (networkRank)
                {
                    case NetworkRank.Alpha:
                        return "α";
                    case NetworkRank.Beta:
                        return "β";
                    case NetworkRank.Gamma:
                        return "γ";
                    case NetworkRank.Delta:
                        return "δ";
                    case NetworkRank.Epsilon:
                        return "ε";
                }
                return "";
            }
        }
    }
}
