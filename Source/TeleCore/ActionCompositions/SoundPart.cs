﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace TeleCore
{
    public struct SoundPart
    {
        public SoundDef def;
        public SoundInfo info;

        public static SoundPart Empty => new SoundPart();

        public SoundPart(SoundDef def, SoundInfo info)
        {
            this.def = def;
            this.info = info;
        }

        public void PlaySound()
        {
            if(def == null) return;
            if (Find.SoundRoot.oneShotManager.CanAddPlayingOneShot(def, info))
                def.PlayOneShot(info);
        }
    }
}
