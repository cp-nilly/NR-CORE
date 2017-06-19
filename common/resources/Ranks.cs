using System.IO;
using Newtonsoft.Json;

namespace common.resources
{
    public class Ranks
    {
        public string role { get; set; }
        public int rank { get; set; }

        public static Ranks[] ReadFile(string fileName)
        {
            using (var r = new StreamReader(fileName))
            {
                return ReadJson(r.ReadToEnd());
            }
        }

        public static Ranks[] ReadJson(string json)
        {
            return JsonConvert.DeserializeObject<Ranks[]>(json);
        }
    }
}
