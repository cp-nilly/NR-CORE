using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.transitions
{
    class HpBoundaryTransition : Transition
    {
        //State storage: none
        class ThresholdState
        {
            public double CurrentThreshold;
            public List<double> Thresholds;
            public int SelectedState;
        }

        private readonly List<double> _thresholds;

        public HpBoundaryTransition(double[] thresholds, string[] targetStates)
            : base(targetStates)
        {
            _thresholds = thresholds.ToList();
        }

        public HpBoundaryTransition(double threshold, string targetState)
            : base(targetState)
        {
            _thresholds = new List<double> { threshold };
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            if (state == null)
            {
                var stateThresholds = _thresholds.ToList();
                state = new ThresholdState()
                {
                    CurrentThreshold = _thresholds[0],
                    Thresholds = stateThresholds,
                    SelectedState = 0
                };
            }

            var tState = state as ThresholdState;

            if (tState.Thresholds == null)
                return false;

            var hpp = (double) (host as Enemy).HP / host.ObjectDesc.MaxHP;
            if (hpp > tState.CurrentThreshold)
                return false;

            SelectedState = tState.SelectedState;

            if (tState.Thresholds.Count <= 1)
                tState.Thresholds = null;
            else
            {
                tState.Thresholds.RemoveAt(0);
                tState.CurrentThreshold = tState.Thresholds[0];
                tState.SelectedState++;
            }

            return true;
        }
    }
}
