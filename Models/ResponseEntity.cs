using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMinerAPI.Models
{
    /// <summary>
    /// this entity is returned by the convert controller. it contains both the text extracted from
    /// the source byte array and the populated search results in json format.
    /// </summary>
    public class ResponseEntity
    {
        public ResponseEntity() { }

        public bool Success { get; set; }

        public string Message { get; set; }

        // document converted to text
        public string DocumentContent { get; set; }

        // documentconent parsed by text processor according to searchset keywords
        public string ParsedContent { get; set; }

        public Guid RequestID { get; set; }

        public string FileName { get; set; }

        public bool DoFormula { get; set; }

    }
}
