using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public class HediffWithGizmos : HediffWithComps
    {
        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}
