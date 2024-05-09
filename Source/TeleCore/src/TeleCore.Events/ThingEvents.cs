using TeleCore.Events.Args;

namespace TeleCore.Events;

//Things
public delegate void ThingSpawnedEvent(ThingStateChangedEventArgs args);
public delegate void ThingDespawnedEvent(ThingStateChangedEventArgs args);
public delegate void ThingStateChangedEvent(ThingStateChangedEventArgs args);
public delegate void TerrainChangedEvent(TerrainChangedEventArgs args);
public delegate void CellChangedEvent(CellChangedEventArgs args);