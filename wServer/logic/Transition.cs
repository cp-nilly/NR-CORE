using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using wServer.realm;

namespace wServer.logic
{
    public abstract class Transition : IStateChildren
    {
        public State[] TargetState { get; private set; }

        protected readonly string[] TargetStates;
        protected int SelectedState;
        
        public Transition(params string[] targetStates)
        {
            TargetStates = targetStates;
        }

        public bool Tick(Entity host, RealmTime time)
        {
            object state;
            host.StateStorage.TryGetValue(this, out state);

            var ret = TickCore(host, time, ref state);
            if (ret)
                host.SwitchTo(TargetState[SelectedState]);

            if (state == null)
                host.StateStorage.Remove(this);
            else
                host.StateStorage[this] = state;
            return ret;
        }

        protected abstract bool TickCore(Entity host, RealmTime time, ref object state);

        internal void Resolve(IDictionary<string, State> states)
        {
            var numStates = TargetStates.Length;
            TargetState = new State[numStates];
            for (var i = 0; i < numStates; i++)
                TargetState[i] = states[TargetStates[i]];
        }

        [ThreadStatic]
        private static Random _rand;
        protected static Random Random
        {
            get { return _rand ?? (_rand = new Random()); }
        }
    }
}
