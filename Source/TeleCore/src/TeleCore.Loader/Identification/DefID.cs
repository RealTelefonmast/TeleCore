using Verse;

namespace TeleCore.Loader;

public readonly struct DefID<TDef> where TDef : Def
{
    public int ID { get; }
    
    public TDef Def => DefIDStack.ToDef<TDef>(ID);
    
    public DefID(int id)
    {
        ID = id;
    }
    
    public static implicit operator DefID<TDef>(int id) => new(id);
    public static implicit operator int(DefID<TDef> defID) => defID.ID;
}