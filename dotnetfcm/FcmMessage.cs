using System;
using System.Collections.Generic;
using System.Text;

namespace dotnetfcm
{
    public class FcmMessage
    {
        public string to { get; set; }
        public Payload notification { get; set; }
        public object data { get; set; }
    }
    public class Payload
    {
        public string title { get; set; }
        public string text { get; set; }
        public string body { get; set; }
    }
}
