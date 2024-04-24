using Verse;

namespace TeleCore.Systems.Events;

public struct FXEffecterSpawnedEventArgs
{
    public string effecterTag;
    public FleckDef fleckDef;
    public Mote mote;
}