using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;  
using DocumentFormat.OpenXml.Spreadsheet;  
using System.Linq;
using Serilog;


namespace DataMinerAPI
{

    public class ServiceSettings
    {

        public ServiceSettings()
        {
            
        }

        public string FilesFolder { get; set; }

        public string WorkingFolder { get; set; }

        public bool DeleteWorkingFiles { get; set; }

    }

}