using System;
using System.Runtime.InteropServices;

namespace TeleCore.Unsafe
{
    public static unsafe class Test
    {
        public static void DoThing()
        {
            //Creating disposable 
            using SelfDispose* myObject = (SelfDispose.Create());
            
            //myObject should dispose itself at the end of the scope
        }
    }

    public unsafe struct DisposeOf<T> : IDisposable where T : DisposeWorker
    {
        private T* _worker;

        public DisposeOf()
        {
            _worker = (T*)Marshal.AllocHGlobal(sizeof(T));
            _worker->Init();
        }
        
        public void Dispose()
        {
            _worker->Dispose();
            Marshal.FreeHGlobal((IntPtr)_worker);
        }
    }
    
    public unsafe class DisposeWorker : IDisposable
    {
        
        ~DisposeWorker()
        {
        }
        
        internal static DisposeWorker* Create()
        {
            return (DisposeWorker*)Marshal.AllocHGlobal(sizeof(DisposeWorker));
        }
        
        public virtual void Init()
        {
            
        }
        
        public void Dispose()
        {
            
        }
    }
}