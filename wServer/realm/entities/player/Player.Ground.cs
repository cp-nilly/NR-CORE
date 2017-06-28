using System;
using System.Linq;
using common.resources;
using wServer.networking.packets.outgoing;
using wServer.realm.terrain;

namespace wServer.realm.entities
{
    public partial class Player
    {
        long l;

        private void HandleOceanTrenchGround(RealmTime time)
        {
            try
            {
                // don't suffocate hidden players
                if (HasConditionEffect(ConditionEffects.Hidden)) return;

                if (time.TotalElapsedMs - l <= 100 || Owner?.Name != "OceanTrench") return;

                if (!(Owner?.StaticObjects.Where(i => i.Value.ObjectType == 0x0731).Count(i => (X - i.Value.X) * (X - i.Value.X) + (Y - i.Value.Y) * (Y - i.Value.Y) < 1) > 0))
                {
                    if (OxygenBar == 0)
                        HP -= 10;
                    else
                        OxygenBar -= 2;

                    if (HP <= 0)
                        Death("suffocation");
                }
                else
                {
                    if (OxygenBar < 100)
                        OxygenBar += 8;
                    if (OxygenBar > 100)
                        OxygenBar = 100;
                }

                l = time.TotalElapsedMs;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        bool HandleGround(RealmTime time)
        {
            if (time.TotalElapsedMs - l > 500)
            {
                if (HasConditionEffect(ConditionEffects.Paused) ||
                    HasConditionEffect(ConditionEffects.Invincible))
                    return false;

                WmapTile tile = Owner.Map[(int)X, (int)Y];
                ObjectDesc objDesc = tile.ObjType == 0 ? null : Manager.Resources.GameData.ObjectDescs[tile.ObjType];
                TileDesc tileDesc = Manager.Resources.GameData.Tiles[tile.TileId];
                if (tileDesc.Damaging && (objDesc == null || !objDesc.ProtectFromGroundDamage))
                {
                    int dmg = (int)Client.Random.NextIntRange((uint)tileDesc.MinDamage, (uint)tileDesc.MaxDamage);

                    HP -= dmg;

                    Owner.BroadcastPacketNearby(new Damage()
                    {
                        TargetId = Id,
                        DamageAmount = (ushort)dmg,
                        Kill = HP <= 0,
                    }, this, this, PacketPriority.Low);

                    if (HP <= 0)
                    {
                        Death(tileDesc.ObjectId, tile:tile);
                        return true;
                    }
                        
                    l = time.TotalElapsedMs;
                }
            }
            return false;
        }

        public void ForceGroundHit(RealmTime time, Position pos, int timeHit)
        {
            if (HasConditionEffect(ConditionEffects.Paused) ||
                HasConditionEffect(ConditionEffects.Invincible))
                return;

            WmapTile tile = Owner.Map[(int) pos.X, (int) pos.Y];
            ObjectDesc objDesc = tile.ObjType == 0 ? null : Manager.Resources.GameData.ObjectDescs[tile.ObjType];
            TileDesc tileDesc = Manager.Resources.GameData.Tiles[tile.TileId];
            if (tileDesc.Damaging && (objDesc == null || !objDesc.ProtectFromGroundDamage))
            {
                int dmg = (int)Client.Random.NextIntRange((uint)tileDesc.MinDamage, (uint)tileDesc.MaxDamage);

                HP -= dmg;

                Owner.BroadcastPacketNearby(new Damage()
                {
                    TargetId = Id,
                    DamageAmount = (ushort)dmg,
                    Kill = HP <= 0,
                }, this, this, PacketPriority.Low);

                if (HP <= 0)
                {
                    Death(tileDesc.ObjectId, tile: tile);
                }
            }
        }
    }
}
