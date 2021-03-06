using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class Comp_FleckThrower : ThingComp
    {
        //On Parent
        private List<FleckThrower> effectThrowers;
        
        //On Parent With Specific Offset
        private List<FleckThrower> effectThrowersOffset;
        private Dictionary<FleckThrower, PositionOffSets> OffsetsForThrowers;
        private Dictionary<FleckThrower, int> TickersForThrowers;

        //
        public CompProperties_FleckThrower Props => base.props as CompProperties_FleckThrower;
        public IFXObject IParent => parent as IFXObject;
        public bool ShouldThrowFlecks => IParent == null || IParent.ShouldThrowFlecks;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            //Create Throwers Direct
            if (!Props.effectThrowers.NullOrEmpty())
            {
                effectThrowers = new();
                foreach (var effectThrower in Props.effectThrowers)
                {
                    effectThrowers.Add(new FleckThrower(effectThrower, parent));
                }
            }
            //Create Throwers With Offset
            if (!Props.effectThrowersOffset.NullOrEmpty())
            {
                effectThrowersOffset = new ();
                OffsetsForThrowers = new ();
                TickersForThrowers = new ();
                foreach (var effectThrower in Props.effectThrowersOffset)
                {
                    var newThrower = new FleckThrower(effectThrower.thrower, parent);
                    effectThrowersOffset.Add(newThrower);
                    OffsetsForThrowers.Add(newThrower, effectThrower.spawnPositions);
                    TickersForThrowers.Add(newThrower, 0);
                }
            }
        }

        public IEnumerable<Vector3> CurrentMoteThrowerOffsetsFor(FleckThrower thrower)
        {
            if (!OffsetsForThrowers.ContainsKey(thrower))
                yield break;

            foreach (var vector in OffsetsForThrowers[thrower][parent.Rotation])
            {
                var v = parent.TrueCenter() + vector;
                yield return new Vector3(v.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v.z);
            }
        }

        /*
        private List<Vector3> MoteOrigins
        {
            get
            {
                if (motePositions.NullOrEmpty() && Props.moteData != null)
                {
                    List<Vector3> positions = new List<Vector3>();
                    Vector3 center = parent.TrueCenter();
                    Vector3 newVec;
                    Rot4 rotation = parent.Rotation;
                    if (rotation == Rot4.North || (rotation == Rot4.South && Props.moteData.southVec.NullOrEmpty()))
                    {
                        for (int i = 0; i < Props.moteData.northVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.northVec[i];
                            newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if (rotation == Rot4.East || (rotation == Rot4.West && Props.moteData.westVec.NullOrEmpty()))
                    {
                        for (int i = 0; i < Props.moteData.eastVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.eastVec[i];
                            newVec = new Vector3(v2.x, AltitudeLayer.MoteOverhead.AltitudeFor(), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if (rotation == Rot4.South)
                    {
                        for (int i = 0; i < Props.moteData.southVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.southVec[i];
                            newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    if (rotation == Rot4.West)
                    {
                        for (int i = 0; i < Props.moteData.westVec.Count; i++)
                        {
                            Vector3 v2 = center + Props.moteData.westVec[i];
                            newVec = new Vector3(v2.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), v2.z);
                            positions.Add(newVec);
                        }
                    }
                    motePositions = positions;
                }
                return motePositions;
            }
        }
        */

        public override void CompTick()
        {
            //Do Effects
            if (!ShouldThrowFlecks) return;

            ThrowFlecksTick();
            ThrowFlecksOffsetTick();
        }

        private void ThrowFlecksTick()
        {
            if (effectThrowers.NullOrEmpty()) return;
            foreach (var fleckThrower in effectThrowers)
            {
                fleckThrower.ThrowerTick(parent.DrawPos, parent.Map);
            }
        }

        private void ThrowFlecksOffsetTick()
        {
            if (effectThrowersOffset.NullOrEmpty()) return;
            for (var i = 0; i < effectThrowersOffset.Count; i++)
            {
                var fleckThrower = effectThrowersOffset[i];
                if (TickersForThrowers[fleckThrower] <= 0)
                {
                    foreach (var vector3 in CurrentMoteThrowerOffsetsFor(fleckThrower))
                    {
                        fleckThrower.ThrowerTick(vector3, parent.Map);
                    }
                    TickersForThrowers[fleckThrower] = Props.effectThrowersOffset[i].tickRange.RandomInRange;
                }
                TickersForThrowers[fleckThrower]--;
            }
        }
    }

    public class CompProperties_FleckThrower : CompProperties
    {
        public List<FleckThrowerProperties> effectThrowersOffset;
        public List<FleckThrowerInfo> effectThrowers;

        public CompProperties_FleckThrower()
        {
            this.compClass = typeof(Comp_FleckThrower);
        }
    }
}
