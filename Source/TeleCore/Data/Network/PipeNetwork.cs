﻿using System;
using System.Collections.Generic;
using TeleCore.Network.Data;
using TeleCore.Network.Flow;
using TeleCore.Network.Graph;
using TeleCore.Network.Utility;
using Verse;
using DebugTools = TeleCore.Static.Utilities.DebugTools;

namespace TeleCore.Network;

public class PipeNetwork : IDisposable
{
    protected NetworkPartSetExtended _partSet;
    private bool DEBUG_DrawFlowPressure;

    //Debug
    private bool DEBUG_DrawGraph;


    public PipeNetwork(NetworkDef def)
    {
        ID = PipeNetworkFactory.MasterNetworkID++;
        NetworkDef = def;
        _partSet = new NetworkPartSetExtended(def);
    }

    public int ID { get; }

    public NetworkDef NetworkDef { get; }

    public NetGraph Graph { get; private set; }

    public FlowSystem FlowSystem { get; private set; }

    public NetworkPartSetExtended PartSet => _partSet;

    public bool IsWorking => !NetworkDef.UsesController || (PartSet.Controller?.IsWorking ?? false);

    public void Dispose()
    {
        Graph.Dispose();
        FlowSystem.Dispose();
        _partSet.Dispose();
    }

    internal void PrepareForRegen(out NetGraph graph, out FlowSystem flow)
    {
        graph = Graph = new NetGraph();
        flow = FlowSystem = new FlowSystem();
    }

    internal bool Notify_AddPart(INetworkPart part)
    {
        return _partSet.AddComponent(part);
    }

    //TODO: not needed right now as networks are always fully re-generated
    /*
    internal void Notify_RemovePart(INetworkPart part)
    {
        ParentSystem.Notify_RemovePart(part);

        //
        PartSet.RemoveComponent(part);
        containerSet.RemoveContainerFrom(part);
        foreach (var cell in part.Parent.Thing.OccupiedRect())
        {
            NetworkCells.Remove(cell);
        }
    }
    */

    internal void Tick()
    {
        FlowSystem.Tick();
        foreach (var part in _partSet.TickSet) part.Tick();
    }

    internal void Draw()
    {
        FlowSystem.Draw();
        Graph.Draw();

        if (DEBUG_DrawFlowPressure) DebugTools.Debug_DrawPressure(FlowSystem);
    }

    internal void OnGUI()
    {
        FlowSystem.OnGUI();
        Graph.OnGUI();

        if (DEBUG_DrawGraph) DebugTools.Debug_DrawGraphOnUI(Graph);
    }

    internal IEnumerable<Gizmo> GetGizmos()
    {
        if (DebugSettings.godMode)
            yield return new Command_Action
            {
                defaultLabel = "Draw Graph",
                defaultDesc = "Renders the graph which represents connections between structures.",
                icon = BaseContent.WhiteTex,
                action = delegate { DEBUG_DrawGraph = !DEBUG_DrawGraph; }
            };
    }
}