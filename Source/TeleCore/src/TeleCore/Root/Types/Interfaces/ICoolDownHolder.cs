﻿namespace TeleCore;

public interface ICoolDownHolder
{
    public bool CoolDownActive { get; }
    
    public float DisabledPct { get; }
}