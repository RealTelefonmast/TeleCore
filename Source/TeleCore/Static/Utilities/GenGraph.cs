﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore.Static.Utilities
{
    internal class GenGraph
    {
        public List<INetworkSubPart> ShortestPathFunction(NetworkGraph graph, INetworkSubPart start, INetworkSubPart end)
        {
            var previous = new Dictionary<INetworkSubPart, INetworkSubPart>();

            var queue = new Queue<INetworkSubPart>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue();
                foreach (var neighbor in graph.AdjacencyLists[vertex])
                {
                    if (previous.ContainsKey(neighbor))
                        continue;

                    previous[neighbor] = vertex;
                    queue.Enqueue(neighbor);
                }
            }

            var path = new List<INetworkSubPart> { };

            var current = end;
            while (!current.Equals(start))
            {
                path.Add(current);
                current = previous[current];
            };

            path.Add(start);
            path.Reverse();

            return path;
        }

        private static List<INetworkSubPart> _WorkingList = new();
        private static Dictionary<INetworkSubPart, int> _Distances = new();
        private static Dictionary<INetworkSubPart, INetworkSubPart> _PreviousOf = new();

        public static List<List<INetworkSubPart>> Dijkstra(NetworkGraph graph, NetworkGraphNodeRequest request)
        {
            return Dijkstra(graph, request.Requester, request.Fits);
        }

        public static List<List<INetworkSubPart>> Dijkstra(NetworkGraph graph, INetworkSubPart source, Predicate<INetworkSubPart> partValidator)
        {
            //
            _WorkingList.Clear();
            _Distances.Clear();
            _PreviousOf.Clear();

            List<INetworkSubPart> validParts = new List<INetworkSubPart>();
            List<List<INetworkSubPart>> allPaths = new List<List<INetworkSubPart>>();
            
            //
            for (var k = 0; k < graph.AllNodes.Count; k++)
            {
                var node = graph.AllNodes[k];
                _WorkingList.Add(node);
                _PreviousOf.Add(node, null);

                if (partValidator(node))
                {
                    validParts.Add(node);
                }

                if (node == source)
                {
                    _Distances.Add(node, 0);
                    continue;
                }
                _Distances.Add(node, int.MaxValue);
            }
            
            while (_WorkingList.Count > 0)
            {
                //
                INetworkSubPart part = null;
                if (validParts.Any())
                {
                    foreach (var toPart in validParts)
                    {
                        part = toPart;
                        if (_PreviousOf[part] != null || part == source)
                        {
                            List<INetworkSubPart> pathResult = new List<INetworkSubPart>();
                            while (part != null)
                            {
                                pathResult.Insert(0, part);
                                part = _PreviousOf[part];
                            }

                            //
                            _WorkingList.Clear();
                            _Distances.Clear();
                            _PreviousOf.Clear();
                            allPaths.Add(pathResult);
                        }
                    }
                    if(allPaths.Count == validParts.Count)
                        return allPaths;
                }

                //
                part = _WorkingList.MinBy(v => _Distances[v]);

                _WorkingList.Remove(part);
                foreach (var neighbor in graph.AdjacencyLists[part])
                {
                    if (!_WorkingList.Contains(neighbor)) continue;
                    if (graph.TryGetEdge(part, neighbor, out var edge))
                    {
                        var alt = _Distances[part] + edge._weight;
                        if (alt < _Distances[neighbor])
                        {
                            _Distances[neighbor] = alt;
                            _PreviousOf[neighbor] = part;
                        }
                    }
                }
            }

            //
            _WorkingList.Clear();
            _Distances.Clear();
            _PreviousOf.Clear();
            return allPaths.Count > 0 ? allPaths : null;
        }
    }
}