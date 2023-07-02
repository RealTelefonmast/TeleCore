﻿using System.Collections.Generic;
using Verse;

namespace TeleCore;

public struct AnimationSet : IExposable
{
    //Layers
    public List<TextureData> textureParts;
    public List<AnimationPart> animations;

    //
    public bool HasTextures => textureParts != null;
    public bool HasAnimations => animations != null;

    //Loading
    public void ExposeData()
    {
        Scribe_Collections.Look(ref textureParts, nameof(textureParts), LookMode.Deep);
        Scribe_Collections.Look(ref animations, nameof(animations), LookMode.Deep);
    }
}