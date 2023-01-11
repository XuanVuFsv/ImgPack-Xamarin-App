using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Models
{
    public class PublishModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("user")]
        public UserModel User { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("image")]
        public string Image { get; set; }
    }
}
