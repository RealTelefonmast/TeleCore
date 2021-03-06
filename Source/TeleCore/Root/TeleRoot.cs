using System;
using UnityEngine;
using Verse;

namespace TeleCore
{
    /// <summary>
    /// Experimental Updating of custom core related parts
    /// </summary>
    public class TeleRoot : MonoBehaviour
    {
        //
        public TeleTickManager TickManager { get; private set; }

        public virtual void Start()
        {
            try
            {
                TickManager = new TeleTickManager();
            }
            catch (Exception arg)
            {
                Log.Error("Error in TiberiumRoot.Start(): " + arg);
            }
        }

        public virtual void Update()
        {
            try
            {
                TickManager?.Update();
            }
            catch (Exception arg)
            {
                Log.Error("Error in TiberiumRoot.Update(): " + arg);
            }
        }
    }
}
