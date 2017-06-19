using wServer.realm;

namespace wServer.logic.transitions
{
    class GroundTransition : Transition
    {
        //State storage: none

        private readonly string _ground;
        private ushort? _groundType;

        public GroundTransition(string ground, string targetState)
            : base(targetState)
        {
            _ground = ground;
        }

        protected override bool TickCore(Entity host, RealmTime time, ref object state)
        {
            if (_groundType == null)
                _groundType = host.Manager.Resources.GameData.IdToTileType[_ground];

            var tile = host.Owner.Map[(int) host.X, (int) host.Y];

            return tile.TileId == _groundType;
        }
    }
}
