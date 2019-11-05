using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMiner.Engine
{
    public class EngineReturnArgs
    {
        public EngineReturnArgs() { }

        public bool Success { get; set; }

        public string Message { get; set; }

        public string Content { get; set; }

        public Exception Exception { get; set; }

    }
}
