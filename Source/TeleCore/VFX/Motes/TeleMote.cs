using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class TeleMote : MoteThrown
    {
        private Material attachedMat;
        protected MaterialPropertyBlock materialProps;

        public float? fadeInTimeOverride;
        public float? fadeOutTimeOverride;

        public override bool EndOfLife => AgeSecs >= LifeSpan;

        private float LifeSpan => FadeInTime + SolidTime + FadeOutTime;
        private float FadeInTime => fadeInTimeOverride ?? def.mote.fadeInTime;
        private float FadeOutTime => fadeOutTimeOverride ?? def.mote.fadeOutTime;

        public Material AttachedMat => attachedMat;

        public override float Alpha
        {
            get
            {
                float ageSecs = this.AgeSecs;
                if (ageSecs <= FadeInTime)
                {
                    if (FadeInTime > 0f)
                    {
                        return ageSecs / FadeInTime;
                    }
                    return 1f;
                }
                else
                {
                    if (ageSecs <= FadeInTime + SolidTime)
                    {
                        return 1f;
                    }
                    if (FadeOutTime > 0f)
                    {
                        return 1f - Mathf.InverseLerp(FadeInTime + SolidTime, LifeSpan, ageSecs);
                    }
                    return 1f;
                }
            }
        }

        public void SetTimeOverrides(float? fadeIn, float? fadeOut)
        {
            fadeInTimeOverride = fadeIn;
            fadeOutTimeOverride = fadeOut;
        }

        public void AttachMaterial(Material newMat, Color color)
        {
            //TLog.Message($"Attaching mat: {newMat}");
            attachedMat = newMat;
            instanceColor = color;
        }
    }
}
