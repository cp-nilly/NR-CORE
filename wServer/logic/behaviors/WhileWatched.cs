using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors
{
    class WhileWatched : CycleBehavior
    {

        Behavior child;
        public WhileWatched(Behavior child)
        {
            this.child = child;
        }

        protected override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            foreach (var player in host.GetNearestEntities(Player.Radius, null, true).OfType<Player>())
                if (player.clientEntities.Contains(host))
                {
                    child.OnStateEntry(host, time);
                    if (child is CycleBehavior)
                        Status = (child as CycleBehavior).Status;
                    else
                        Status = CycleStatus.InProgress;
                    return;
                }
            Status = CycleStatus.NotStarted;
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            foreach (var player in host.GetNearestEntities(Player.Radius, null, true).OfType<Player>())
                if (player.clientEntities.Contains(host))
                {
                    child.Tick(host, time);
                    if (child is CycleBehavior)
                        Status = (child as CycleBehavior).Status;
                    else
                        Status = CycleStatus.InProgress;
                    return;
                }
            Status = CycleStatus.NotStarted;
        }

        protected override void OnStateExit(Entity host, RealmTime time, ref object state)
        {
            foreach (var player in host.GetNearestEntities(Player.Radius, null, true).OfType<Player>())
                if (player.clientEntities.Contains(host))
                {
                    child.OnStateExit(host, time);
                    if (child is CycleBehavior)
                        Status = (child as CycleBehavior).Status;
                    else
                        Status = CycleStatus.InProgress;
                    return;
                }
            Status = CycleStatus.NotStarted;
        }
    }
}
