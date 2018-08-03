using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EventReader
{
    public class Room
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Address")]
        public string Address { get; set; }
    }

    public class RoomList
    {
        [JsonProperty(PropertyName = "@odata.context")]
        public string odatacontext { get; set; }

        [JsonProperty(PropertyName = "value")]
        public List<Room> Rooms { get; set; }
    }
}

