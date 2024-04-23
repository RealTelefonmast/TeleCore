﻿using TeleCore.Generics.Container.Holder;
using TeleCore.Network;

namespace TeleCore.Generics.Container;

//Container Template implementing IContainerHolder
public abstract class ValueContainerWithHolder<TValue, THolder> : ValueContainerBase<TValue>
    where TValue : FlowValueDef
    where THolder : IContainerHolderBase<TValue>
{
    protected ValueContainerWithHolder(ContainerConfig<TValue> config, THolder holder) : base(config)
    {
        Holder = holder;
    }

    public THolder Holder { get; }

    public override void Notify_ContainerStateChanged(NotifyContainerChangedArgs<TValue> stateChangeArgs)
    {
        Holder?.Notify_ContainerStateChanged(stateChangeArgs);
    }
}