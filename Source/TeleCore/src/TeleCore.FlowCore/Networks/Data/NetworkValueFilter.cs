using System.Collections.Generic;
using Verse;

namespace TeleCore.FlowCore;

public class NetworkValueFilter : FlowValueFilter<Flow.Values.NetworkValueDef>
{
    public List<NetworkValueFilterByRole> allowedValuesByRole;

    [Unsaved]
    private Dictionary<NetworkRole, List<Flow.Values.NetworkValueDef>>? allowedValuesByRoleInt;

    public List<Flow.Values.NetworkValueDef> this[NetworkRole role] => allowedValuesByRoleInt[role];


    /// <summary>
    ///     Provides sub-managed values by role, if set in the networkRole props.
    /// </summary>
    public Dictionary<NetworkRole, List<Flow.Values.NetworkValueDef>> AllowedValuesByRole
    {
        get
        {
            if (allowedValuesByRoleInt != null) return allowedValuesByRoleInt;

            allowedValuesByRoleInt = new Dictionary<NetworkRole, List<Flow.Values.NetworkValueDef>>();
            foreach (var filter in allowedValuesByRole)
                if (filter.HasSubValues && filter != NetworkRole.Transmitter)
                    allowedValuesByRoleInt.Add(filter, filter.subValues);
            return allowedValuesByRoleInt;
        }
    }


    public void PostLoad()
    {
    }
}