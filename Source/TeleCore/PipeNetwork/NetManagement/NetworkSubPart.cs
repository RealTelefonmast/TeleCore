﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeleCore.Network;
using TeleCore.Network.IO;
using TeleCore.Static.Utilities;
using UnityEngine;
using Verse;

namespace TeleCore;

//TODO: Refer to factorios fluidbox improvement by creating memory-efficient container logic
//Bind parts to their containers in a network managing class
public class NetworkSubPart : IExposable, INetworkSubPart, INetworkRequester, IContainerHolderNetwork
{
    //
    protected NetworkCellIO cellIO;
    protected NetworkContainer container;
    protected NetworkPartSet directPartSet;

    //Role Workers
    private NetworkRequestWorker requestWorkerInt;
        
    private NetworkDef internalDef;
    private int lastReceivedTick;
    private int receivingTicks;
    private bool drawNetworkInfo = false;

    //DEBUG
    protected bool DebugNetworkCells = false;

    //
    public NetworkSubPartProperties Props { get; private set; }
    public Thing Thing => Parent.Thing;
    public bool ShowStorageGizmo { get; }
    public bool ShowStorageForThingGizmo => false;

    public NetworkDef NetworkDef => internalDef;
    public NetworkRole NetworkRole => Props.NetworkRole;
    public PipeNetwork Network { get; set; }
    //public INetworkSubPart Part { get; set; }
    public INetworkStructure Parent { get; private set; }
    public INetworkSubPart NetworkPart => this;

    //
    public bool Initialized => Network?.Initialized ?? false;
        
    //States
    public bool IsMainController => Network?.NetworkController == Parent;
    public bool IsNetworkNode => NetworkRole != NetworkRole.Transmitter;// || IsJunction; //|| IsPipeEndPoint;
    public bool IsNetworkEdge => !IsNetworkNode;
    public bool IsJunction => NetworkRole == NetworkRole.Transmitter && DirectPartSet[NetworkRole.Transmitter]?.Count > 2;
    public bool IsPipeEndPoint => NetworkRole == NetworkRole.Transmitter && DirectPartSet[NetworkRole.Transmitter]?.Count == 1;
    public bool IsReceiving => receivingTicks > 0;
    public bool NetworkActive => Network.IsWorking || !Props.requiresController;
        
    //
    public bool HasContainer => Props.containerConfig != null;
    public bool HasConnection => DirectPartSet[NetworkRole.Transmitter]?.Count > 0;
    public bool HasLeak => false;

    //Sub Role Handling
    public Dictionary<NetworkRole, List<NetworkValueDef>> ValuesByRole => Props.AllowedValuesByRole;

    //
    public NetworkPartSet DirectPartSet => directPartSet;

    public NetworkCellIO CellIO
    {
        get
        {
            if (cellIO != null) return cellIO;
            if (Props.subIOPattern == null) return Parent.GeneralIO;
            return cellIO ??= new NetworkCellIO(Props.subIOPattern, Parent.Thing);
        }
    }

    //

    //Container
    public string ContainerTitle => "_Obsolete_";
    public NetworkContainerSet ContainerSet => Network.ContainerSet;

    //BaseContainer<NetworkValueDef, IContainerHolder<NetworkValueDef>> IContainerHolder<NetworkValueDef>.Container => Container;
        
    public NetworkContainer Container
    {
        get => container;
        private set => container = value;
    }

    //Role Workers
    public NetworkRequestWorker RequestWorker => requestWorkerInt;

    #region Constructors

    public NetworkSubPart(){}
        
    public NetworkSubPart(INetworkStructure parent)
    {
        Parent = parent;
    }
        
    public NetworkSubPart(INetworkStructure parent, NetworkSubPartProperties properties)
    {
        Parent = parent;
        Props = properties;
        internalDef = properties.networkDef;
    }

    #endregion

    public virtual void ExposeData()
    {
        Scribe_Defs.Look(ref internalDef, "internalDef");
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            //TODO: Clean this up, shouldnt cast to comp
            Props = ( (Comp_Network)Parent).Props.networks.Find(p => p.networkDef == internalDef);
        }
            
        if (Props.containerConfig != null)
        {
            //Scribe_Values.Look(ref requestedCapacityPercent, "requesterPercent");
            //Scribe_Values.Look(ref requestedCpacityRange, "requestedCpacityRange");
            Scribe_Container.Look<NetworkContainer, NetworkValueDef>(ref container, Props.containerConfig, this, "container");
        }
            
        Scribe_Deep.Look(ref requestWorkerInt, nameof(requestWorkerInt), this);
    }

    public void SubPartSetup(bool respawningAfterLoad)
    {
        //Generate components
        directPartSet = new NetworkPartSet(NetworkDef, this);

        RolePropertySetup(respawningAfterLoad);
        GetDirectlyAdjacentNetworkParts();
            
        if (respawningAfterLoad) return; // IGNORING EXPOSED CONSTRUCTORS
        if (HasContainer)
            Container = new NetworkContainer(Props.containerConfig, this);
    }

    private void GetDirectlyAdjacentNetworkParts()
    {
        for (var c = 0; c < CellIO.OuterConnnectionCells.Length; c++)
        {
            IntVec3 connectionCell = CellIO.OuterConnnectionCells[c];
            List<Thing> thingList = connectionCell.GetThingList(Thing.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (PipeNetworkMaker.Fits(thingList[i], NetworkDef, out var subPart))
                {
                    if (ConnectsTo(subPart))
                    {
                        directPartSet.AddNewComponent(subPart);
                        subPart.DirectPartSet.AddNewComponent(this);
                    }
                }
            }
        }
    }

    public void PostDestroy(DestroyMode mode, Map previousMap)
    {
        DirectPartSet.Notify_ParentDestroyed();
        Container?.Notify_ParentDestroyed(mode, previousMap);
        //Network.Notify_RemovePart(this);
    }

    private void RolePropertySetup(bool respawningAfterLoad)
    {
        if (respawningAfterLoad) return;
            
        if ((NetworkRole & NetworkRole.Requester) == NetworkRole.Requester)
        {
            requestWorkerInt = new NetworkRequestWorker(this);
        }
    }

    //
    public void NetworkTick()
    {
        if (!Initialized) return;
        if (IsNetworkEdge)
        {
            TLog.Warning("Edges should not be ticked!");
            return;
        }
        
        var isPowered = Parent.IsPowered;
        if (isPowered && Parent.IsWorking)
        {
            if (receivingTicks > 0 && lastReceivedTick < Find.TickManager.TicksGame)
                receivingTicks--;

            if (!NetworkActive) return;
            
            //Producers push to Storages
            if ((NetworkRole & NetworkRole.Producer) == NetworkRole.Producer && Parent.RoleIsActive(NetworkRole.Producer))
            {
                ProducerTick();
            }

            //Storages push to Consumers
            if ((NetworkRole & NetworkRole.Storage) == NetworkRole.Storage && Parent.RoleIsActive(NetworkRole.Storage))
            {
                StorageTick();
            }

            //Consumers slowly use up own container
            if ((NetworkRole & NetworkRole.Consumer) == NetworkRole.Consumer && Parent.RoleIsActive(NetworkRole.Consumer))
            {
                ConsumerTick();
            }

            //
            if ((NetworkRole & NetworkRole.Requester) == NetworkRole.Requester && Parent.RoleIsActive(NetworkRole.Requester))
            {
                RequesterTick();
            }
        }
        Parent.NetworkPostTick(this, isPowered);
    }
    
    protected virtual void ProducerTick()
    {
        var adjacencyList = Network.Graph.GetAdjacencyList(this);
        var adjCount = adjacencyList.Count;
        var stackPart = container.ValueStack / adjCount;

        float totalExcess = 0;
        float totalDeficit = 0;

        // Calculate the total excess and deficit amounts
        foreach (var adjPart in adjacencyList)
        {
            float difference = adjPart.Container.TotalStored - stackPart.TotalValue.AsT0;
            if (difference > 0)
                totalExcess += difference;
            else
                totalDeficit += Math.Abs(difference);
        }

        foreach (var adjPart in adjacencyList)
        {
            float difference = adjPart.Container.TotalStored - stackPart.TotalValue.AsT0;
            float ratio = difference > 0 ? difference / totalExcess : Math.Abs(difference) / totalDeficit;

            foreach (var value in container.ValueStack)
            {
                int amountToDistribute = Mathf.RoundToInt(value.ValueInt * ratio);
                var result = difference > 0 ? adjPart.Container.TryAddValue(value, amountToDistribute) : adjPart.Container.TryRemoveValue(value, amountToDistribute);
                if (result)
                {
                    if (difference > 0)
                        container.TryRemoveValue(value, result.ActualAmount);
                    else
                        container.TryAddValue(value, result.ActualAmount);
                }
            }
        }
    }

    protected virtual void StorageTick()
    {
        if (Container.ContainsForbiddenType)
        {
            if (Container.FillState == ContainerFillState.Empty) return;
            NetworkTransactionUtility.DoTransaction(new TransactionRequest(this,
                NetworkRole.Storage, NetworkRole.Consumer|NetworkRole.Storage,
                part => NetworkTransactionUtility.Actions.TransferToOtherPurge(this, part),
                part => NetworkTransactionUtility.Validators.PartValidator_Sender(this, part,
                    ePart => FlowValueUtils.CanExchangeForbidden(Container, ePart.Container))));
        }
            
        if (Container.Config.storeEvenly && Network.HasGraph)
        {
            NetworkTransactionUtility.DoTransaction(new TransactionRequest(this,
                NetworkRole.Storage, NetworkRole.Storage,
                part => NetworkTransactionUtility.Actions.TransferToOther_Equalize(this, part),
                part => NetworkTransactionUtility.Validators.StoreEvenly_EQ_Check(this, part)));
            return;
        }
            
        //
        NetworkTransactionUtility.DoTransaction(new TransactionRequest(this,
            NetworkRole.Storage, NetworkRole.Consumer,
            part => NetworkTransactionUtility.Actions.TransferToOther_AnyDesired(this, part),
            part => NetworkTransactionUtility.Validators.PartValidator_Sender(this, part)));
    }

    protected virtual void ConsumerTick()
    {
    }

    /// <summary>
    /// When using the Requester NetworkRole, will pull values in according to <see cref="RequestedCpacityRange"/>.
    /// </summary>
    protected virtual void RequesterTick()
    {
        if (RequestWorker.RequestTick())
        {
            //When set to automatic, adjusts settings to pull equal amounts of each value-type
            if (RequestWorker.Mode == RequesterMode.Automatic)
            {
                //Resolve..
                //var maxVal = RequestedCapacityPercent * Container.Capacity;
                var maxPercent = RequestWorker.ReqRange.max; //Get max percentage
                foreach (var valType in Props.AllowedValuesByRole[NetworkRole.Requester])
                {
                    //var requestedTypeValue = container.ValueForType(valType);
                    var requestedTypeNetworkValue = Network.TotalValueFor(valType, NetworkRole.Storage);
                    if (requestedTypeNetworkValue > 0)
                    {
                        var setValue = Mathf.Min(maxPercent,
                            requestedTypeNetworkValue / Container.Capacity);
                        var tempVal = RequestWorker.Settings[valType];

                        tempVal.Item2 = setValue;
                        RequestWorker.Settings[valType] = tempVal;
                        maxPercent = Mathf.Clamp(maxPercent - setValue, 0, maxPercent);
                    }
                }
            }

            //Pull values according to settings
            //if (container.StoredPercent >= RequestedCapacityPercent) return;
            foreach (var setting in RequestWorker.Settings)
            {
                //If not requested, skip
                if (!setting.Value.isActive) continue;
                if (container.StoredPercentOf(setting.Key) < setting.Value.desiredAmt)
                {
                    NetworkTransactionUtility.DoTransaction(new TransactionRequest(this,
                        NetworkRole.Requester, NetworkRole.Storage,
                        part =>
                        {
                            var partContainer = part.Container;
                            if (partContainer.FillState == ContainerFillState.Empty) return;
                            if (partContainer.StoredValueOf(setting.Key) <= 0) return;
                            if (partContainer.TryTransferValue(container, setting.Key, 1, out _))
                            {
                                _ = true;
                            }
                        }));
                }
            }
        }
    }

    private void DoNetworkAction(INetworkSubPart fromPart, INetworkSubPart previous, NetworkRole ofRole, Action<INetworkSubPart> funcOnPart, Predicate<INetworkSubPart> validator)
    {
        var adjacencyList = fromPart.Network.Graph.GetAdjacencyList(this);
        if (adjacencyList == null) return;
            
        foreach (var subPart in adjacencyList)
        {
            if (subPart == previous) continue;

            if (subPart.IsJunction)
            {
                DoNetworkAction(subPart, fromPart, ofRole, funcOnPart, validator);
                continue;
            }
                
            if ((subPart.NetworkRole & ofRole) != ofRole) continue;
            if(!validator(subPart)) continue;
            funcOnPart(subPart);
        }
    }

    //Network Transaction Logic
    public void Notify_ReceivePackage(TransactionPackage package)
    {
        
    }
    
    //
    private void SubTransfer(INetworkSubPart previousPart, INetworkSubPart part, List<NetworkValueDef> usedTypes, NetworkRole ofRole)
    {
        var adjacencyList = part.Network.Graph.GetAdjacencyList(this);
        if (adjacencyList == null) return;
            
        foreach (var subPart in adjacencyList)
        {
            if(subPart == previousPart) continue;
                
            if (subPart.IsJunction)
            {
                SubTransfer(part, subPart, usedTypes, ofRole);
                continue;
            }

            if ((subPart.NetworkRole & ofRole) != ofRole) continue;
            if (Container.FillState == ContainerFillState.Empty || subPart.Container.FillState == ContainerFillState.Full) continue;
            for (int i = usedTypes.Count - 1; i >= 0; i--)
            {
                var type = usedTypes[i];
                if (!subPart.NeedsValue(type, ofRole)) continue;
                if (Container.TryTransferValue(subPart.Container, type, 1, out _))
                {
                    subPart.Notify_ReceivedValue();
                }
            }
        }
    }
        
    private void Processor_Transfer(INetworkSubPart part, NetworkRole fromRole, NetworkRole ofRole)
    {
        if (part == null)
        {
            TLog.Warning("Target part of path is null");
            return;
        }
            
        var usedTypes = Props.AllowedValuesByRole[fromRole];
        for (int i = usedTypes.Count - 1; i >= 0; i--)
        {
            var type = usedTypes[i];
            if (!part.NeedsValue(type, ofRole)) continue;
            if (Container.TryTransferValue(part.Container, type, 1, out _))
            {
                part.Notify_ReceivedValue();
            }
        }
    }

    //Data Notifiers
    public virtual void Notify_ContainerStateChanged(NotifyContainerChangedArgs<NetworkValueDef> args)
    {
            
    }

    public void Notify_ReceivedValue()
    {
        lastReceivedTick = Find.TickManager.TicksGame;
        receivingTicks++;
        Parent.Notify_ReceivedValue();
    }

    public void Notify_StateChanged(string signal)
    {
        if (signal is "FlickedOn" or "FlickedOff")
        {
            //...
        }
    }

    public void Notify_SetConnection(NetEdge edge, IntVec3Rot ioCell)
    {
    }

    public void Notify_NetworkDestroyed()
    {
    }

    //
    public void SendFirstValue(INetworkSubPart other)
    {
        Container.TryTransferValue(other.Container, Container.StoredDefs.FirstOrDefault(), 1, out _);
    }

    //
    public bool CanInteractWith(INetworkSubPart other)
    {
        return Parent.CanInteractWith(other);
    }
    
    public IOConnectionResult ConnectsTo(INetworkSubPart other)
    {
        if (other == this) return IOConnectionResult.Invalid;
        if (!NetworkDef.Equals(other.NetworkDef)) return IOConnectionResult.Invalid;
        if (!Parent.CanConnectToOther(other.Parent)) return IOConnectionResult.Invalid;
        return CellIO.ConnectsTo(other.CellIO);
    }

    /*
    private bool CompatibleWith(INetworkSubPart other)
    {
        if (other.Network == null)
        {
            TLog.Error($"{other.Parent.Thing} is not part of any Network - this should not be the case.");
            return false;
        }
        return other.Network.NetworkRank == Network.NetworkRank;
    }
    */
        
    public bool CanTransmit(NetEdge netEdge)
    {
        return (NetworkRole & NetworkRole.Transmitter) == NetworkRole.Transmitter;
    }

    public bool AcceptsValue(NetworkValueDef value)
    {
        if (HasContainer && Parent.AcceptsValue(value))
        {
            if (Container.CanReceiveValue(value))
            {
                return true;
            }
        }
        return false;
    }
        
    public bool NeedsValue(NetworkValueDef value, NetworkRole forRole)
    {
        if (Props.AllowedValuesByRole.TryGetValue(forRole, out var values) && values.Contains(value))
        {
            return AcceptsValue(value);
        }
        return false;
    }

    //Gizmos
    public virtual IEnumerable<Gizmo> GetPartGizmos()
    {

        /*
        if (HasContainer)
        {
            foreach (var containerGizmo in Container.GetGizmos())
            {
                yield return containerGizmo;
            }
        }
        */

        /*
        foreach (var networkGizmo in GetSpecialNetworkGizmos())
        {
            yield return networkGizmo;
        }
        */

        if (DebugSettings.godMode)
        {
            if (IsMainController)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Show Entire Network",
                    action = delegate
                    {
                        DebugNetworkCells = !DebugNetworkCells;
                    }
                };
            }

            if (Network == null) yield break;
                
            yield return new Command_Action
            {
                defaultLabel = $"Draw Graph",
                action = delegate { Network.DrawInternalGraph = !Network.DrawInternalGraph; }
            };

            yield return new Command_Action
            {
                defaultLabel = $"Draw AdjacencyList",
                action = delegate { Network.DrawAdjacencyList = !Network.DrawAdjacencyList; }
            };
                
                
            yield return new Command_Action
            {
                defaultLabel = $"Draw FlowDirections",
                action = delegate { Debug_DrawFlowDir = !Debug_DrawFlowDir; }
            };
        }
    }

    //Readouts and UI
    public virtual string NetworkInspectString()
    {
        StringBuilder sb = new StringBuilder();
        if (DebugSettings.godMode)
        {
            
            //TODO: Move to Debug ITab
            sb.AppendLine($"[{NetworkDef}]ID: {Network?.ID.ToString()}");
            sb.AppendLine($"Has Network: {(Network != null).ToString()}");
            sb.AppendLine($"Network Valid: {Network?.IsValid.ToString()}");
            if (IsNetworkNode)
            {
                sb.AppendLine("Is Node");
            }

            if (IsNetworkEdge)
            {
                sb.AppendLine("Is Edge");
            }

            if (IsJunction)
            {
                sb.AppendLine("Is Junction");
            }

            sb.AppendLine($"[Transmitters] {DirectPartSet[NetworkRole.Transmitter]?.Count}");
        }

        return sb.ToString().TrimEndNewlines();
    }
        
    internal static bool Debug_DrawFlowDir = false;

    private Rot4 FlowDir
    {
        get;
        set;
    } = Rot4.Invalid;

    internal void SetFlowDir(INetworkSubPart other)
    {
        /*
        if (Network.Graph.TryGetEdge(this, other, out var edge))
        {
            FlowDir = edge.fromToDir;
        }
        */
    }
        
    public void Draw()
    {
        DrawNetworkInfo();
        if (DebugNetworkCells)
        {
            GenDraw.DrawFieldEdges(Network.NetworkCells, Color.cyan);
        }

        //Render Flow Debug
        if (Debug_DrawFlowDir && FlowDir != Rot4.Invalid)
        {
            var matrix = new Matrix4x4();
            matrix.SetTRS(Parent.Thing.DrawPos, 0f.ToQuat(), new Vector3(1, AltitudeLayer.MetaOverlays.AltitudeFor(), 1));
            Graphics.DrawMesh(MeshPool.plane10, matrix, TeleContent.IOArrowRed, 0);
        }
    }

    private void DrawNetworkInfo()
    {
        /*
        if (!drawNetworkInfo) return;
        Rect sizeRect = new Rect(UI.screenWidth / 2 - (756/2),UI.screenHeight/2 - (756/2), 756, 756);
        Find.WindowStack.ImmediateWindow(GetHashCode(), sizeRect, WindowLayer.GameUI, () =>
        {
            int row = 0;
            float curY = 0;

            foreach (var container in Network.ContainerSet[NetworkRole.All])
            {
                Widgets.Label(new Rect(0, curY, 150, 20), $"{keyValue.Key}: ");
                int column = 0;
                curY += 20;
                foreach (var container in keyValue.Value)
                {
                    Rect compRect = new Rect(column * 100 + 5, curY, 100, 100);
                    Widgets.DrawBox(compRect);
                    string text = $"{container.Parent.Thing.def}:\n";

                    TWidgets.DrawTiberiumReadout(compRect, container);
                    column++;
                }
                row++;
                curY += 100 + 5;
            }
            
            foreach (var structures in Network.PartSet.StructuresByRole)
            {
                Widgets.Label(new Rect(0, curY, 150, 20), $"{structures.Key}: ");
                int column = 0;
                curY += 20;
                foreach (var component in structures.Value)
                {
                    Rect compRect = new Rect(column * 100 + 5, curY, 100, 100);
                    Widgets.DrawBox(compRect);
                    string text = $"{component.Parent.Thing.def}:\n";
                    switch (structures.Key)
                    {
                        case NetworkRole.Producer:
                            text = $"{text}Producing:";
                            break;
                        case NetworkRole.Storage:
                            text = $"{text}";
                            break;
                        case NetworkRole.Consumer:
                            text = $"{text}";
                            break;
                        case NetworkRole.Requester:
                            text = $"{text}";
                            break;
                    }
                    Widgets.Label(compRect, $"{text}");
                    column++;
                }
                row++;
                curY += 100 + 5;
            }
            
        } );
    */
    }

    //
    public override string ToString()
    {
        return Parent.Thing.ToString();
    }
}