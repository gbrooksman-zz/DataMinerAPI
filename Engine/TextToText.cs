using System;
using System.IO;
using System.Text;
using System.Linq;
using Serilog;
using DataMinerAPI.Models;


namespace DataMinerAPI.Engine
{
    public class TextToText
	{

       public TextToText()
       {
           
       }

        public EngineReturnArgs ConvertTextToText(string conversionSource, Guid requestGuid, string fileExtension) 
        {
            EngineReturnArgs era = new EngineReturnArgs();
            era.RequestID = requestGuid;

            try
            {
                string textContent = System.IO.File.ReadAllText(conversionSource);
                era.DocumentContent = textContent;
				era.Success = true;
				era.Message = "Conversion ok";
            }
            catch(Exception ex)
            {
                Log.Error(ex, "In ConvertTextToText");
				era.Success = false;
				era.Message = "Conversion failed";
				era.DocumentContent = ex.Message;
            }

            return era;
        }
    }
}