using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PORO.Models
{
    public class ResponseModel<TData>
    {
        public TData Result { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public bool Error { get; set; }
    }
    public class ResponseModel
    {
        [JsonProperty("result")]
        public string Result { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }
    }
}
