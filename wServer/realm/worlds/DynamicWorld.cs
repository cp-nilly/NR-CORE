using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using common.resources;
using DungeonGenerator;
using DungeonGenerator.Templates;
using wServer.networking;

namespace wServer.realm.worlds
{
    public static class DynamicWorld
    {
        private static readonly List<Type> Worlds;

        static DynamicWorld()
        {
            Worlds = new List<Type>();

            var type = typeof (World);
            var worlds = type.Assembly.GetTypes()
                .Where(t => type.IsAssignableFrom(t) && type != t);

            foreach (var i in worlds)
                Worlds.Add(i);
        }

        public static void TryGetWorld(ProtoWorld wData, Client client, out World world)
        {
            world = null;

            foreach (var type in Worlds)
            {
                if (!type.Name.Equals(wData.name))
                    continue;

                world = (World)Activator.CreateInstance(type, wData, client);
                return;
            }
        }
    }
}
