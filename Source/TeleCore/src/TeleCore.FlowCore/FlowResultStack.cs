using TeleCore.Shared;

namespace TeleCore.FlowCore;

public struct FlowResultStack<TDef>
    where TDef : FlowValueDef
{
    public DefValueStack<TDef, double> Desired { get; }
    public DefValueStack<TDef, double> Actual { get; private set; }
    public FlowFailureReason Reason { get; private set; }

    public DefValueStack<TDef, double> Diff => Desired - Actual;
    private double DiffValue => Desired.TotalValue - Actual.TotalValue;

    public FlowState State
    {
        get
        {
            if (Reason != FlowFailureReason.None)
                return FlowState.Failed;

            if (DiffValue <= double.Epsilon)
                return FlowState.Completed;
            if (DiffValue > 0)
                return FlowState.CompletedWithExcess;
            if (DiffValue < 0)
                return FlowState.CompletedWithShortage;

            return FlowState.Failed;
        }
    }

    public static implicit operator bool(FlowResultStack<TDef> result) => result.State != FlowState.Failed;

    private FlowResultStack(DefValueStack<TDef, double> desired)
    {
        Desired = desired;
    }

    public static FlowResultStack<TDef> Init(DefValueStack<TDef, double> desired, FlowOperation opType)
    {
        if (opType == FlowOperation.Remove)
            desired *= -1;
        return new FlowResultStack<TDef>(desired);
    }

    public FlowResultStack<TDef> AddResult(DefValue<TDef, double> result)
    {
        Actual += result;
        return this;
    }

    public FlowResultStack<TDef> AddResult(FlowResult<TDef, double> subResult)
    {
        Actual += (subResult.Def, subResult.Actual);
        return this;
    }

    public FlowResultStack<TDef> Fail(FlowFailureReason reason)
    {
        Reason = reason;
        return this;
    }
}