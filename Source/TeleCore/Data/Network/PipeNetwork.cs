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

    public NetworkGraph Graph { get; private set; }

    public NetworkSystem NetworkSystem { get; private set; }

    public NetworkPartSetExtended PartSet => _partSet;

    public bool IsWorking => !NetworkDef.UsesController || (PartSet.Controller?.IsWorking ?? false);

    public void Dispose()
    {
        Graph.Dispose();
        NetworkSystem.Dispose();
        _partSet.Dispose();
    }

    internal void Prepare()
    {
        Graph = new NetworkGraph();
        NetworkSystem = new NetworkSystem();
    }

    internal void PrepareForRegen(out NetworkGraph graph, out NetworkSystem networkSystem)
    {
        graph = Graph = new NetworkGraph();
        networkSystem = NetworkSystem = new NetworkSystem();
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

    public void TickSystem(int tick)
    {
        NetworkSystem.Tick(tick);
    }
    
    internal void Tick(int tick)
    {
        foreach (var part in _partSet.TickSet)
            part.Tick();
    }

    internal void Draw()
    {
        NetworkSystem.Draw();
        Graph.Draw();
        
        if (DEBUG_DrawFlowPressure)
            DebugTools.Debug_DrawPressure(NetworkSystem);
    }

    internal void OnGUI()
    {
        NetworkSystem.OnGUI();
        Graph.OnGUI();
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