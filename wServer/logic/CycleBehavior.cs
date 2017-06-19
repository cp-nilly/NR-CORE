using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.logic
{
    enum CycleStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
    abstract class CycleBehavior : Behavior
    {
        public CycleStatus Status { get; protected set; }
    }
}
