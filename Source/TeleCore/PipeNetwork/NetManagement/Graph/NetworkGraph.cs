﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    [StaticConstructorOnStartup]
    public class NetworkGraph
    {
        //
        private static readonly Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(Color.green, ShaderDatabase.MetaOverlay);
        private static readonly Material UnFilledMat = SolidColorMaterials.NewSolidColorMaterial(TColor.LightBlack, ShaderDatabase.MetaOverlay);

        //Graph Data
        private readonly List<INetworkSubPart> _allNodes;
        private readonly Dictionary<INetworkSubPart, LinkedList<INetworkSubPart>> _adjacencyLists;
        private readonly Dictionary<(INetworkSubPart, INetworkSubPart), NetEdge> _edges;
        
        //TODO: Contemplate edge caching directly
        private readonly Dictionary<INetworkSubPart, List<(INetworkSubPart, NetEdge)>> _adjacencyListEdge;

        //Props
        public int NodeCount => _adjacencyLists.Count;
        public int EdgeCount => _edges.Count;

        public List<INetworkSubPart> AllNodes => _allNodes;
        private Dictionary<INetworkSubPart, LinkedList<INetworkSubPart>> AdjacencyLists => _adjacencyLists;
        public PipeNetwork ParentNetwork { get; internal set; }

        public NetworkGraph()
        {
            _allNodes = new List<INetworkSubPart>();
            _adjacencyListEdge = new Dictionary<INetworkSubPart, List<(INetworkSubPart, NetEdge)>>();
            _adjacencyLists = new Dictionary<INetworkSubPart, LinkedList<INetworkSubPart>>();
            _edges = new Dictionary<(INetworkSubPart, INetworkSubPart), NetEdge>();
        }

        public void Notify_StateChanged(INetworkSubPart part)
        {
        }

        //
        public LinkedList<INetworkSubPart>? GetAdjacencyList(INetworkSubPart forPart)
        {
            if (_adjacencyLists.TryGetValue(forPart, out var list))
            {
                return list;
            }
            return null;
        }
        
        public IEnumerable<(INetworkSubPart,NetEdge)> GetAdjacencyListEdge(INetworkSubPart forPart)
        {
            if (_adjacencyListEdge.TryGetValue(forPart, out var list))
            {
                return list;
            }
            return null;
        }

        public void AddNode(INetworkSubPart node)
        {
            _allNodes.Add(node);
            _adjacencyLists.Add(node, new LinkedList<INetworkSubPart>());
            _adjacencyListEdge.Add(node, new List<(INetworkSubPart, NetEdge)>());
        }

        public bool AddEdge(NetEdge newEdge)
        {
            var newKey = (fromNode: newEdge.startNode, toNode: newEdge.endNode);
            if (_edges.ContainsKey(newKey))
            {
                TLog.Warning($"Key ({newEdge.startNode.Parent.Thing}, {newEdge.endNode.Parent.Thing}) already exists in graph!");
                return false;
            }

            if (newEdge.IsValid)
            {
                _edges.Add(newKey, newEdge);
                if (!_adjacencyLists.TryGetValue(newEdge.startNode, out var listSource))
                {
                    AddNode(newEdge.startNode);
                    listSource = _adjacencyLists[newEdge.startNode];
                }
                if (!listSource.Contains(newEdge.endNode))
                {
                    listSource.AddFirst(newEdge.endNode);
                    _adjacencyListEdge[newEdge.startNode].Add((newEdge.endNode, newEdge));
                }
            }
            return true;
        }
        
        /*
        public bool AddEdge(NetEdge newEdge)
        {
            var newKey = (fromNode: newEdge.startNode, toNode: newEdge.endNode);
            var reverseKey = (newEdge.endNode, newEdge.startNode);
            if (_edges.ContainsKey(newKey) && _edges.ContainsKey(reverseKey))
            {
                TLog.Warning($"Key ({newEdge.startNode.Parent.Thing}, {newEdge.endNode.Parent.Thing}) already exists in graph!");
                return false;
            }
            
            _edges.Add(newKey, newEdge);

            if (!_adjacencyLists.TryGetValue(newEdge.startNode, out var listSource))
            {
                AddNode(newEdge.startNode);
                listSource = _adjacencyLists[newEdge.startNode];
            }

            LinkedList<INetworkSubPart> listSource2 = null;
            if (newEdge.IsBiDirectional)
            {
                if (!_adjacencyLists.TryGetValue(newEdge.endNode, out listSource2))
                {
                    AddNode(newEdge.endNode);
                    listSource2 = _adjacencyLists[newEdge.endNode];
                }
            }

            if (!listSource.Contains(newEdge.endNode))
            {
                listSource.AddFirst(newEdge.endNode);
                _adjacencyListEdge[newEdge.startNode].Add((newEdge.endNode, newEdge));
            }

            //Only add a mirrored access key if the edge is bi-directional
            if (newEdge.IsBiDirectional)
            {
                if (!listSource2.Contains(newEdge.startNode))
                {
                    listSource2?.AddFirst(newEdge.startNode);
                    _adjacencyListEdge[newEdge.endNode].Add((newEdge.startNode, newEdge.Reverse));
                }
            }
            return true;
        }
        */

        //
        public bool HasKnownEdgeFor(INetworkSubPart fromRoot, IntVec3 cell, out NetEdge netEdge)
        {
            netEdge = new NetEdge();
            return false;
            //fromRoot.AdjacencySet
        }

        public bool GetAnyEdgeBetween(INetworkSubPart source, INetworkSubPart dest, out NetEdge value)
        {
            value = GetEdgeFor(source, dest, true);
            return value.IsValid;
        }
        
        public bool TryGetEdge(INetworkSubPart source, INetworkSubPart dest, out NetEdge value)
        {
            value = GetEdgeFor(source, dest);
            return value.IsValid;
            return _edges.TryGetValue((source, dest), out value);// || _edges.TryGetValue((dest, source), out value);
        }

        private NetEdge GetEdgeFor(INetworkSubPart source, INetworkSubPart dest, bool any = false)
        {
            if (_edges.TryGetValue((source, dest), out var value))
            {
                return value;
            }

            if (_edges.TryGetValue((dest, source), out value))
            {
                return any ? value : value.Reverse;
            }

            return NetEdge.Invalid;
        }
        
        public IEnumerable<NetEdge> EdgesFor(INetworkSubPart startNode)
        {
            foreach (var adjacentNode in _adjacencyLists.TryGetValue(startNode))
            {
                if (_edges.TryGetValue((startNode, adjacentNode), out var edge))
                    yield return edge;
            }
        }

        internal void Debug_DrawGraphOnUI()
        {
            var size = Find.CameraDriver.CellSizePixels / 4;
            /*
            foreach (var pair in _edgePairs)
            {
                var edge1 = pair.Item1;
                var edge2 = pair.Item2;
                
                if(edge1.IsValid)
                    TWidgets.DrawHalfArrow(edge1.fromNode.Parent.Thing.TrueCenter().ToScreenPos(), edge1.toNode.Parent.Thing.TrueCenter().ToScreenPos(), Color.red, size);
                if(edge2.IsValid)
                    TWidgets.DrawHalfArrow(edge2.fromNode.Parent.Thing.TrueCenter().ToScreenPos(), edge2.toNode.Parent.Thing.TrueCenter().ToScreenPos(), Color.blue, size);
            }
            */
            
            foreach (var netEdge in _edges)
            {
                var subParts = netEdge.Key;
                var thingA = subParts.Item1.Parent.Thing;
                var thingB = subParts.Item2.Parent.Thing;
                
                //TWidgets.DrawHalfArrow(ScreenPositionOf(thingA.TrueCenter()), ScreenPositionOf(thingB.TrueCenter()), Color.red, 8);

                //TODO: edge access only works for one version (node1, node2) - breaks two-way
                //TODO: some edges probably get setup broken (because only one edge is set)
                if (netEdge.Value.IsValid)
                {
                    TWidgets.DrawHalfArrow(netEdge.Value.startNode.Parent.Thing.TrueCenter().ToScreenPos(), netEdge.Value.endNode.Parent.Thing.TrueCenter().ToScreenPos(), Color.red, size);
                    if (netEdge.Value.IsBiDirectional)
                    {
                        TWidgets.DrawHalfArrow(netEdge.Value.endNode.Parent.Thing.TrueCenter().ToScreenPos(), netEdge.Value.startNode.Parent.Thing.TrueCenter().ToScreenPos(), Color.blue, size);  
                    }
                }
                TWidgets.DrawBoxOnThing(thingA);
                TWidgets.DrawBoxOnThing(thingB);
            }
        }

        internal void Debug_DrawPressure()
        {
            foreach (var networkSubPart in AllNodes)
            {
                if(!networkSubPart.HasContainer) continue;
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = networkSubPart.Parent.Thing.Position.ToVector3() + new Vector3(0.25f, 0, 0.75f);
                r.size = new Vector2(1.5f, 0.5f);
                r.fillPercent = networkSubPart.Container.StoredPercent;
                r.filledMat = FilledMat;
                r.unfilledMat = UnFilledMat;
                r.margin = 0f;
                r.rotation = Rot4.East;
                GenDraw.DrawFillableBar(r);
            }
        }

        public void Debug_DrawOverlays()
        {
            foreach (var networkSubPart in AllNodes)
            {
                var pos = networkSubPart.Parent.Thing.DrawPos;
                GenMapUI.DrawText(new Vector2(pos.x, pos.z), $"[{networkSubPart.Parent.Thing}]", Color.green);
            }
        }
    }
}
