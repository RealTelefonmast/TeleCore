﻿using RimWorld.Planet;
using Verse;

namespace TeleCore;

public class WorldComp_TeleCore : WorldComponent
{
    //Discovery
    internal DiscoveryTable discoveries;

    public WorldComp_TeleCore(World world) : base(world)
    {
        GenerateInfos();
        StaticData.Notify_NewTeleWorldComp(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        StaticData.ExposeStaticData();
        Scribe_Deep.Look(ref discoveries, "DiscoveryTable");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) 
            GenerateInfos();
    }


    private void GenerateInfos()
    {
        discoveries ??= new DiscoveryTable();
    }
}