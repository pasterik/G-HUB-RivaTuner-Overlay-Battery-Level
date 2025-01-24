using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHUB_Overlay.Model
{
    public struct GHUBMsg
    {
        public string? MsgId { get; set; }
        public string? Verb { get; set; }
        public string? Path { get; set; }
        public string? Origin { get; set; }
        public JObject? Result { get; set; }
        public JObject? Payload { get; set; }

        public static GHUBMsg DeserializeJson(string json)
        {
            return JsonConvert.DeserializeObject<GHUBMsg>(json);
        }
    }
}
