using System.Linq;
using DataMinerAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using Serilog;
using System.Text.Json;
using System.IO;
using System.Text;
using System;

namespace DataMinerAPI.Engine
{
    public class BatchOutputBuilder
	{ 
        private readonly ServiceSettings settings;
		private readonly IMemoryCache cache;

		public BatchOutputBuilder(IMemoryCache _cache, ServiceSettings _settings)
		{
			settings = _settings;
			cache = _cache;
		}

        public void AddBatchItem(EngineReturnArgs entity)
        {
           var obj = JsonSerializer.Deserialize<SearchSet>(entity.ParsedContent);

           var serializer = new System.Xml.Serialization.XmlSerializer(typeof(SearchSet));
            
            string fileName = $"{settings.FilesFolder}out/batchconvert.xml";

            using(var strW = new StringWriter())
            {
               serializer.Serialize(strW,obj); 

                using (StreamWriter sw = File.AppendText(fileName)) 
                {
                    sw.Write(strW.ToString());
                    sw.WriteLine();
                }	
            }
        }

        public void InitOutputFiles()
        {
            string fileName = $"{settings.FilesFolder}out/batchconvert.xml";

            if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Truncate);
                fs.Close();
            }

            fileName = $"{settings.FilesFolder}out/result_log.txt";

            if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Truncate);
                fs.Close();
            }
        }

    }
    

}