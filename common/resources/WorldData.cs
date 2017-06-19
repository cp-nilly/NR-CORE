using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using Newtonsoft.Json;
using terrain;

namespace common.resources
{
    public struct ProtoWorld
    {
        public String name;
        public String sbName;
        public int id;
        public int difficulty;
        public int background;
        public bool isLimbo;
        public bool restrictTp;
        public bool showDisplays;
        public bool persist;
        public int blocking;
        public bool setpiece;
        public int[] portals;
        public string[] maps;
        public string[] music;
        public byte[][] wmap;
    }

    public class WorldData
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(WorldData));

        public IDictionary<string, ProtoWorld> Data { get; private set; }

        public WorldData(string dir, XmlData gameData)
        {
            Dictionary<string, ProtoWorld> worlds;
            Data =
                new ReadOnlyDictionary<string, ProtoWorld>(
                    worlds = new Dictionary<string, ProtoWorld>());

            // load up worlds
            string basePath = Path.GetFullPath(dir);
            var jwFiles = Directory.EnumerateFiles(basePath, "*.jw", SearchOption.TopDirectoryOnly).ToArray();
            for (var i = 0; i < jwFiles.Length; i++)
            {
                Log.InfoFormat("Initializing world data: " + Path.GetFileName(jwFiles[i]) + " {0}/{1}...", i + 1, jwFiles.Length);
                
                var jw = File.ReadAllText(jwFiles[i]);
                var world = JsonConvert.DeserializeObject<ProtoWorld>(jw);

                if (world.maps == null)
                {
                    var jm = File.ReadAllText(jwFiles[i].Substring(0, jwFiles[i].Length - 1) + "m");
                    world.wmap = new byte[1][];
                    world.wmap[0] = Json2Wmap.Convert(gameData, jm);
                    worlds.Add(world.name, world);
                    continue;
                }
               
                world.wmap = new byte[world.maps.Length][];
                var di = Directory.GetParent(jwFiles[i]);
                for (var j = 0; j < world.maps.Length; j++)
                {
                    var mapFile = Path.Combine(di.FullName, world.maps[j]);
                    if (world.maps[j].EndsWith(".wmap"))
                        world.wmap[j] = File.ReadAllBytes(mapFile);
                    else
                    {
                        var jm = File.ReadAllText(mapFile);
                        world.wmap[j] = Json2Wmap.Convert(gameData, jm);
                    }
                }
                worlds.Add(world.name, world);
            }
        }

        public ProtoWorld this [string name] 
        {
            get { return Data[name]; }
        }
    }
}
