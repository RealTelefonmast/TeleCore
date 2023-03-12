﻿using Verse;

namespace TeleCore
{
    public abstract class MapInformation : IExposable
    {
        protected Map map;
        private bool initialized = false;

        //
        public bool HasBeenInitialized => initialized;
        public Map Map => map;

        protected MapInformation(Map map)
        {
            this.map = map;
        }

        /// <summary>
        /// Initializier run on MapComponent.FinalizeInit
        /// </summary>
        /// <param name="initAfterReload"></param>
        public virtual void InfoInit(bool initAfterReload = false)
        {
            initialized = true;
        }

        /// <summary>
        /// Thread safe initializer for data on the main game thread
        /// </summary>
        public virtual void ThreadSafeInit()
        {
        }

        /// <summary>
        /// Runs on MapComponent.MapGenerated
        /// </summary>
        public virtual void OnMapGenerated()
        {
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref initialized, "mapInfoInit");
        }

        public virtual void Tick()
        {
        }

        /// <summary>
        /// This tick is called by the TeleRoot Monobehaviour, offloading work from the RW Root.
        /// </summary>
        public virtual void TeleTick()
        {
            
        }

        /// <summary>
        /// Allows to run code on each cell on a map, similar to GameConditions.
        /// </summary>
        /// <param name="c">Affected Celll</param>
        public virtual void CellSteadyEffect(IntVec3 c)
        {
        }

        /// <summary>
        /// Allows to run custom UI render code.
        /// </summary>
        public virtual void UpdateOnGUI()
        {
        }

        /// <summary>
        /// Allows to run custom map rendering code.
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// This update is called from the TeleRoot Monobehaviour, offloading rendering from the RW Root.
        /// </summary>
        public virtual void TeleUpdate()
        {
            
        }
    }
}
