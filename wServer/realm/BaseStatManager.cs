using common;

namespace wServer.realm
{
    class BaseStatManager
    {
        private readonly StatsManager _parent;
        private readonly int[] _base;

        public BaseStatManager(StatsManager parent)
        {
            _parent = parent;
            _base = Utils.ResizeArray(
                parent.Owner.Client.Character.Stats, 
                StatsManager.NumStatTypes);
            
            ReCalculateValues();
        }

        public int[] GetStats()
        {
            return (int[]) _base.Clone();
        }

        public int this[int index]
        {
            get { return _base[index]; }
            set
            {
                _base[index] = value;
                _parent.StatChanged(index);
            }
        }

        protected internal void ReCalculateValues(InventoryChangedEventArgs e = null)
        {
            SetWeaponDamage(e);
        }

        private void SetWeaponDamage(InventoryChangedEventArgs e)
        {
            var min = 0;
            var max = 0;
            var weap = _parent.Owner.Inventory[0];
            if (weap != null && weap.Projectiles.Length > 0)
            {
                min = weap.Projectiles[0].MinDamage;
                max = weap.Projectiles[0].MaxDamage;
            }

            _base[8] = min;
            _base[9] = max;
        }
    }
}
