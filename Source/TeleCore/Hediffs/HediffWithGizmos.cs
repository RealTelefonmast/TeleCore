﻿using System.Collections.Generic;
using Verse;

namespace TeleCore;

public class HediffWithGizmos : HediffWithComps
{
    public virtual IEnumerable<Gizmo> GetGizmos()
    {
        yield break;
    }
}