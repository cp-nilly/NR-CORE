using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common.resources;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.behaviors.PetBehaviors
{
    public abstract class PetBehavior : Behavior
    {
        private readonly bool requiresPlayerOwner;

        protected PAbility AbilityType { get; }
        protected PetAbility Ability { get; private set; }
        protected Pet Pet { get; private set; }
        protected Player PlayerOwner => Pet.PlayerOwner;

        protected PetBehavior(PAbility ability, bool requiresPlayerOwner)
        {
            this.requiresPlayerOwner = requiresPlayerOwner;
            AbilityType = ability;
        }

        protected sealed override void OnStateEntry(Entity host, RealmTime time, ref object state)
        {
            if (!(host is Pet)) return;
            state = 0;
            Setup((Pet)host);
            base.OnStateEntry(host, time, ref state);
        }

        protected sealed override void OnStateExit(Entity host, RealmTime time, ref object state)
        {
            base.OnStateExit(host, time, ref state);
        }

        private void Setup(Pet pet)
        {
            Pet = pet;
            Ability = Pet.Ability.FirstOrDefault(_ => _.Type == AbilityType);
        }

        protected override void TickCore(Entity host, RealmTime time, ref object state)
        {
            if(!Check()) return;
            TickCore(Pet, time, ref state);
        }

        protected abstract void TickCore(Pet host, RealmTime time, ref object state);

        private bool Check()
        {
            return Pet?.Owner != null && (requiresPlayerOwner && Pet.PlayerOwner?.Owner != null) && Ability != default(PetAbility);
        }

        protected int DiminishingReturnsCooldown(int min, int max, int level, double scale)
        {
            if (level < 0)
                throw new ArgumentException("Value can't be lesser than 0", nameof(level));
            var mult = level / scale;
            var trinum = (Math.Sqrt(8.0 * mult + 1.0) - 1.0) / 2.0;
            return (int)((max - (trinum * scale) + min) * 1000);
        }
    }
}
