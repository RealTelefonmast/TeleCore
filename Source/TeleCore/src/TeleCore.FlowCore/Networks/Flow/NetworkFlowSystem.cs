using System;
using TeleCore.FlowCore.Flow.Clamping;
using TeleCore.FlowCore.Flow.Pressure;
using TeleCore.FlowCore.Graph;
using UnityEngine;

namespace TeleCore.FlowCore.Flow;

/// <summary>
/// The main algorithm for fluid flow.
/// </summary>
public class NetworkFlowSystem : FlowSystem<NetworkPart, NetworkVolume, Values.NetworkValueDef>
{
    public ClampWorker ClampWorker { get; set; }
    public PressureWorker PressureWorker { get; set; }

    public NetworkFlowSystem()
    {
        ClampWorker = new ClampWorker_Overcommit();
        PressureWorker = new PressureWorker_WaveEquationDamping3();
    }

    protected override float GetInterfacePassThrough(TwoWayKey<NetworkPart> connectors)
    {
        return Mathf.Min(connectors.A.PassThrough, connectors.B.PassThrough);
    }

    protected override NetworkVolume CreateVolume(NetworkPart part)
    {
        if (part.CachedVolume != null)
        {
            return part.CachedVolume;
        }
        if (part.Config.volumeConfig == null)
        {
            TLog.Error("Tried to create a NetworkVolume without a volumeConfig!");
            return null;
        }
        return new NetworkVolume(part.Config.volumeConfig);
    }

    internal void Notify_NewNode(NetworkPart part)
    {
        if (part.Config.volumeConfig != null)
        {
            GenerateForOrGetVolume(part);
        }
    }

    internal void Notify_Populate(NetEdge newEdge)
    {
        var edge = newEdge;
        if (!edge.IsValid)
        {
            TLog.Error($"Tried to populate net-system with invalid edge: {edge}");
            return;
        }
        if (edge.IsLogical) return;
        var fb1 = GenerateForOrGetVolume(edge.From);
        var fb2 = GenerateForOrGetVolume(edge.To);
        if (fb1 == null || fb2 == null)
        {
            TLog.Warning("Null volume created!");
            return;
        }
        var mode = edge.BiDirectional ? InterfaceFlowMode.TwoWay : InterfaceFlowMode.FromTo;
        var iFace = new FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef>(edge.From, edge.To, fb1, fb2, mode);
        AddInterface((edge.From, edge.To), iFace);
    }

    internal void Notify_NetNodeRemoved(NetworkPart part)
    {
        TryRemoveRelatedPart(part);
    }

    internal void Notify_Populate(NetworkGraph graph)
    {
        foreach (var edgePair in graph.EdgesByNodes)
        {
            var edge = graph.GetBestEdgeFor(edgePair.Key);
            if (!edge.IsValid)
            {
                TLog.Error($"Tried to populate net-system with invalid edge: {edge}");
                continue;
            }
            if (edge.IsLogical) continue;
            var fb1 = GenerateForOrGetVolume(edge.From);
            var fb2 = GenerateForOrGetVolume(edge.To);
            if (fb1 == null || fb2 == null)
            {
                TLog.Warning("Null volume created!");
                continue;
            }
            var mode = edge.BiDirectional ? InterfaceFlowMode.TwoWay : InterfaceFlowMode.FromTo;
            var iFace = new FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef>(edge.From, edge.To, fb1, fb2, mode);
            AddInterface((edge.From, edge.To), iFace);
        }
    }

    public double TotalValueFor(Values.NetworkValueDef def)
    {
        return TotalStack[def].Value;
    }

    public double TotalValueFor(Values.NetworkValueDef def, NetworkRole role)
    {
        //TODO: Implement
        return 0;
    }

    public void TryAddValue(NetworkVolume fb, FlowValueDef def, double amount)
    {
        throw new NotImplementedException();
    }

    public void TryRemoveValue(NetworkVolume fb, FlowValueDef def, double amount)
    {
        throw new NotImplementedException();
    }

    public bool TryConsume(NetworkVolume fb, Values.NetworkValueDef def, double value)
    {
        return false;
    }

    public void Clear(NetworkVolume fb)
    {
        throw new NotImplementedException();
    }

    internal void Draw()
    {
    }

    internal void OnGUI()
    {
    }

    protected override DefValueStack<Values.NetworkValueDef, double> FlowFunc(FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef> connection, DefValueStack<Values.NetworkValueDef, double> previous)
    {
        return PressureWorker.FlowFunction(connection, previous);
    }

    protected override DefValueStack<Values.NetworkValueDef, double> ClampFunc(FlowInterface<NetworkPart, NetworkVolume, Values.NetworkValueDef> iface, DefValueStack<Values.NetworkValueDef, double> flow, ClampType clampType)
    {
        return ClampWorker.ClampFunction(iface, flow, clampType);
    }

    // protected override double FlowFunc(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double previous)
    // {
    //     return PressureWorker.FlowFunction(iface, previous);
    // }

    // protected override double ClampFunc(FlowInterface<NetworkPart, NetworkVolume, NetworkValueDef> iface, double flow, ClampType clampType)
    // {
    //     return ClampWorker.ClampFunction(iface, flow, clampType);
    // }
}