using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace common.resources
{
    public class Tinker
    {
        [JsonIgnore]
        public uint DbId { get; }

        public int OwnerId { get; set; }
        public int Goal { get; set; }
        public int Tier { get; set; }

        internal Tinker(uint dbId, Tinker tinker)
        {
            DbId = dbId;
            OwnerId = tinker.OwnerId;
            Goal = tinker.Goal;
            Tier = tinker.Tier;
        }

        private Tinker()
        {
            
        }

        internal static Tinker CreateNew(int ownerId, int goal, int tier)
        {
            return new Tinker
            {
                OwnerId = ownerId,
                Goal = goal,
                Tier = tier
            };
        }

        internal static Tinker Load(uint dbId, string jsonData)
        {
            var serializer = new JsonSerializer();
            var tinker = serializer.Deserialize<Tinker>(new JsonTextReader(new StringReader(jsonData)));
            return new Tinker(dbId, tinker);
        }

        [JsonIgnore]
        public int ByteLength
        {
            get
            {
                var serializer = new JsonSerializer();
                var wtr = new StringWriter();
                serializer.Serialize(wtr, this);
                Bytes = Encoding.UTF8.GetBytes(wtr.ToString());
                return Bytes.Length;
            }
        }
        [JsonIgnore]
        public byte[] Bytes { get; private set; }

        public static int RandomGoal(XmlData gameData)
        {
            return gameData.Items.Select(_ => _.Value).PickRandom().ObjectType;
        }
    }
}
