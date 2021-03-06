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
    public class BeamProperties
    {
        [Unsaved]
        private Material cachedBeamMat;

        //
        public DamageDef damageDef;
        public int damageBase = 100;
        public float armorPenetration;
        public int damageTicks = 10;
        public float stoppingPower;
        public float staggerTime = 95.TicksToSeconds();

        //
        public EffecterDef impactEffecter;
        public ExplosionProperties impactExplosion;
        public FilthSpewerProperties impactFilth;

        //Graphical
        public string beamTexturePath;
        //public FloatRange scratchRange = new FloatRange(3, 4);

        public float fadeInTime = 0.25f;
        public float solidTime = 0.25f;
        public float fadeOutTime = 0.85f;

        public Material BeamMat => cachedBeamMat ??= MaterialPool.MatFrom(beamTexturePath, ShaderDatabase.MoteGlow);
    }
}
