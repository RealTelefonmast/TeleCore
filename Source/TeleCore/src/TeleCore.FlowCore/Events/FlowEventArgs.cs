using TeleCore.Shared;

namespace TeleCore.FlowCore.Events;

public struct FlowEventArgs
{
    public DefValue<Flow.Values.NetworkValueDef, double> Value { get; private set; }
    
    public FlowEventArgs(DefValue<Flow.Values.NetworkValueDef, double> valueChange)
    {
        Value = valueChange;
    }
}