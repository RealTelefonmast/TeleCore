using Verse;

namespace TeleCore.Systems.Events;

public struct ProjectileLaunchedArgs
{
    public Projectile Projectile { get; }
    
    public ProjectileLaunchedArgs(Projectile projectile)
    {
        Projectile = projectile;
    }
}