using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMinerAPI.Models
{
    public class EngineReturnArgs
    {
        public EngineReturnArgs() { }

        public bool Success { get; set; }

        public string Message { get; set; }

        // document converted to text
        public string DocumentContent { get; set; }

        // documentconent parsed by text processor according to searchset keywords
        public string ParsedContent { get; set; }

        public Exception Exception { get; set; }

        public Guid RequestID { get; set; }

    }
}
