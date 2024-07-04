﻿using System.Collections.Generic;
using System.Xml;
using Verse;

namespace TeleCore.FlowCore;

/// <summary>
///     <para>Allows to set a network value filter for a specific role.</para>
///     <para>For example: Storage role allows to contain type A, B, C</para>
///     <para>While Requester role only allows to request B, C</para>
/// </summary>
public class NetworkValueFilterByRole
{
    public NetworkRole role = NetworkRole.Transmitter;
    public List<Flow.Values.NetworkValueDef> subValues;

    public NetworkValueFilterByRole()
    {
    }

    public NetworkValueFilterByRole(NetworkRole networkRole)
    {
        role = networkRole;
    }

    public bool HasSubValues => subValues != null;

    public static implicit operator NetworkRole(NetworkValueFilterByRole props)
    {
        return props.role;
    }

    public static implicit operator NetworkValueFilterByRole(NetworkRole role)
    {
        return new NetworkValueFilterByRole(role);
    }

    public bool IsRole(NetworkRole role)
    {
        return this == role;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //
        if (xmlRoot.Name != "li")
        {
            TLog.Error("Definition for networkRole not a listing.");
            return;
        }

        if (xmlRoot.ChildNodes.Count == 1)
        {
            role = ParseHelper.FromString<NetworkRole>(xmlRoot.InnerText);
            return;
        }

        role = DirectXmlToObject.ObjectFromXml<NetworkRole>(xmlRoot.SelectSingleNode(nameof(role)), false);
        subValues = DirectXmlToObject.ObjectFromXml<List<Flow.Values.NetworkValueDef>>(xmlRoot.SelectSingleNode(nameof(subValues)),
            true);
    }

    public override string ToString()
    {
        return role.ToString();
    }
}