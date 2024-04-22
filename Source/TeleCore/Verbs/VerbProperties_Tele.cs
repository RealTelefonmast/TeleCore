﻿using System.Collections.Generic;
using TeleCore.Network.Bills;
using UnityEngine;
using Verse;

namespace TeleCore;

public class VerbProperties_Tele : VerbProperties
{
    public List<VerbCompProperties> comps;
    
    //Functional
    public Vector3 shotStartOffset = Vector3.zero;
    public List<Vector3>? originOffsetPerShot;
    
    //Effects
    public MuzzleFlashProperties muzzleFlash;
    public EffecterDef originEffecter;
    
    //
    public NetworkCost networkCostPerShot;
    public float powerConsumptionPerShot = 0;
}