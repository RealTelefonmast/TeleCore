﻿using System.Collections.Generic;
using TeleCore.FlowCore;
using UnityEngine;
using Verse;

namespace TeleCore;

public class FlowValueDef : Def
{
    internal static readonly Dictionary<string, List<FlowValueDef>> TaggedFlowValues = new ();
    
    public FlowValueCollectionDef collectionDef;
    
    public float capacityFactor = 1;
    public string labelShort;
    public bool sharesCapacity;
    public Color valueColor = Color.white;
    public string valueUnit;
    
    //The rate at which value flows between containers
    public float viscosity = 1;
    public double friction;

    public List<string> tags;
    
    //Runtime
    public float FlowRate => 1f / viscosity;

    public string ToUnitString(double value)
    {
        return $"{value}{valueUnit}";
    }
    
    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors()) yield return error;

        if (viscosity == 0) yield return $"{nameof(viscosity)} cannot be 0!";
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        if (labelShort.NullOrEmpty())
            labelShort = label;

        collectionDef?.Notify_ResolvedFlowValueDef(this);

        if (!tags.NullOrEmpty())
        {
            foreach (var tag in tags)
            {
                if (!TaggedFlowValues.ContainsKey(tag))
                {
                    TaggedFlowValues.Add(tag, new List<FlowValueDef>());
                }
                TaggedFlowValues[tag].Add(this);
            }
        }
    }
}