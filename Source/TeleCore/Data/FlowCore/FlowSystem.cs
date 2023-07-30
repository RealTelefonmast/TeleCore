﻿using System;
using System.Collections.Generic;
using TeleCore.FlowCore.Events;
using TeleCore.Network.Flow.Clamping;
using TeleCore.Primitive;
using Verse;

namespace TeleCore.FlowCore;

public struct TwoWayKey<TAttach>
{
    private TAttach A { get; set; }
    private TAttach B { get; set; }

    public static implicit operator TwoWayKey<TAttach>((TAttach, TAttach) tuple)
    {
        return new TwoWayKey<TAttach>(tuple.Item1, tuple.Item2);
    }
    
    private TwoWayKey(TAttach a, TAttach b)
    {
        A = a;
        B = b;
    }
    
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;

            hash = hash * 31 + (A == null ? 0 : A.GetHashCode());
            hash = hash * 31 + (B == null ? 0 : B.GetHashCode());

            return hash;
        }
    }
}

public abstract class FlowSystem<TAttach, TVolume, TValueDef> : IDisposable
where TValueDef : FlowValueDef
where TVolume : FlowVolume<TValueDef>
{
    private readonly List<TVolume> _volumes;
    private readonly List<FlowInterface<TVolume, TValueDef>> _interfaces;
    private readonly Dictionary<TAttach, TVolume> _relations;
    private readonly Dictionary<TVolume, List<FlowInterface<TVolume, TValueDef>>> _connections;
    private readonly Dictionary<TwoWayKey<TAttach>, FlowInterface<TVolume, TValueDef>> _interfaceLookUp;
    private DefValueStack<TValueDef, double> _totalStack;
    
    public List<TVolume> Volumes => _volumes;
    public List<FlowInterface<TVolume, TValueDef>> Interfaces => _interfaces;
    public Dictionary<TAttach, TVolume> Relations => _relations;
    public Dictionary<TVolume, List<FlowInterface<TVolume, TValueDef>>> Connections => _connections;
    public DefValueStack<TValueDef, double> TotalStack => _totalStack;
    public double TotalValue => _totalStack.TotalValue;

    public FlowSystem()
    {
        _volumes = new List<TVolume>();
        _interfaces = new List<FlowInterface<TVolume, TValueDef>>();
        _relations = new Dictionary<TAttach, TVolume>();
        _connections = new Dictionary<TVolume, List<FlowInterface<TVolume, TValueDef>>>();
        _interfaceLookUp = new();
    }
    
    public void Dispose()
    {
        _volumes.Clear();
        Relations.Clear();
        Connections.Clear();
    }

    protected TVolume GenerateForOrGet(TAttach part)
    {
        if (Relations.TryGetValue(part, out var volume))
        {
            return volume;
        }
        
        volume = CreateVolume(part);
        volume.FlowEvent += OnFlowBoxEvent;
        Volumes.Add(volume);
        Relations.Add(part, volume);
        return volume;
    }

    protected abstract TVolume CreateVolume(TAttach part);

    protected virtual void OnFlowBoxEvent(object sender, FlowEventArgs e)
    {
    }
    
    protected virtual void PreTickProcessor(int tick)
    {
    }

    //TODO: Add in custom flowsystems
    public void Notify_CreateInterface( TwoWayKey<TAttach> connectors, FlowInterface<TVolume, TValueDef> iface)
    {
        _interfaces.Add(iface);
        _interfaceLookUp.Add(connectors, iface);
    }
    
    public void Tick(int tick)
    {
        PreTickProcessor(tick);
        
        foreach (var _volume in _volumes)
        {
            _volume.PrevStack = _volume.Stack;
        }
        
        foreach (var conn in _interfaces)
        {
            UpdateFlow(conn);
        }

        foreach (var volume in _interfaces)
        {
            UpdateContent(volume);
        }
        
        foreach (var volume in _volumes)
        {
            UpdateFlowRate(volume);
        }
    }
    
    public abstract double FlowFunc(FlowInterface<TVolume, TValueDef> connection, double flow);
    public abstract double ClampFunc(FlowInterface<TVolume, TValueDef> connection, double flow, ClampType clampType);
    
    public void UpdateFlow(FlowInterface<TVolume, TValueDef> iface)
    {
        if (iface.PassPercent <= 0)
        {
            iface.NextFlow = 0;
            iface.Move = 0;
            return;
        }
            
        var flow = iface.NextFlow;      

        flow = FlowFunc(iface, flow);
        iface.UpdateBasedOnFlow(flow);
        flow = Math.Abs(flow);
        iface.NextFlow = ClampFunc(iface, flow, ClampType.FlowSpeed);
        iface.Move = ClampFunc(iface, flow, ClampType.FluidMove);
    }
    
    private void UpdateContent(FlowInterface<TVolume, TValueDef> conn)
    {
        DefValueStack<TValueDef, double> res = conn.From.RemoveContent(conn.Move);
        conn.To.AddContent(res);
        //Console.WriteLine($"Moved: " + conn.Move + $":\n{res}");
        //TODO: Structify for: _connections[fb][i] = conn;
    }

    private void UpdateFlowRate(TVolume fb)
    {
        double fp = 0;
        double fn = 0;

        if (!_connections.TryGetValue(fb, out var conns)) return;
        foreach (var conn in conns)
            Add(conn.Move);

        fb.FlowRate = Math.Max(fp, fn);
        return;

        void Add(double f)
        {
            if (f > 0)
                fp += f;
            else
                fn -= f;
        }
    }
}