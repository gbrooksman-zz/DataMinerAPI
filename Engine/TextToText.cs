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

        public ResponseEntity ConvertTextToText(string conversionSource, Guid requestGuid) 
        {
            ResponseEntity respEntity = new ResponseEntity();
            respEntity.RequestID = requestGuid;

            try
            {
                string textContent = System.IO.File.ReadAllText(conversionSource);
                respEntity.DocumentContent = textContent;
				respEntity.Success = true;
				respEntity.Message = "Conversion ok";
            }
            catch(Exception ex)
            {
                Log.Error(ex, "In ConvertTextToText");
				respEntity.Success = false;
				respEntity.Message = "Conversion failed";
				respEntity.DocumentContent = ex.Message;
            }

            return respEntity;
        }
    }
}