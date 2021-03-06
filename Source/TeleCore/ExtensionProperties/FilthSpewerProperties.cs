using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TeleCore
{
    public class FilthSpewerProperties
    {
        public List<DefFloat<ThingDef>> filths;
        public float spreadRadius = 1.9f;

        public void SpawnFilth(IntVec3 center, Map map)
        {
            foreach (var cell in GenRadial.RadialCellsAround(center, spreadRadius, true))
            {
                foreach (var filth in filths)
                {
                    if (Rand.Chance(filth.value))
                    {
                        FilthMaker.TryMakeFilth(cell, map, filth.def, 1);
                        break;
                    }
                }
            }
        }
    }
}
