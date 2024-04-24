using System;

namespace TeleCore;

public struct DisposableAction : IDisposable
{
    private Action action;

    public DisposableAction(Action action)
    {
        this.action = action;
    }

    public void Dispose()
    {
        action = null;
    }

    public void DoAction()
    {
        action.Invoke();
        Dispose();
    }
}