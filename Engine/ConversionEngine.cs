using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using DataMinerAPI.Models;
using System.Text.Json;
using System.IO;

namespace DataMinerAPI.Engine
{
	/// <summary>
	///
	/// </summary>
	public class ConversionEngine
	{
        private readonly IMemoryCache cache;
		private readonly ServiceSettings settings;

        private readonly string inputDir;
         private readonly string workingDir;


        public ConversionEngine(IMemoryCache _cache, ServiceSettings _settings)
		{
			cache = _cache;
			settings = _settings;

            inputDir = settings.FilesFolder;
			workingDir = settings.WorkingFolder;
        }

        public EngineReturnArgs ConvertDocumentFromFile(string fileName, string keywordsJSON, string application)
        {
            EngineReturnArgs retArgs = new EngineReturnArgs();

            retArgs.RequestID =  Guid.NewGuid();              

            Log.Debug($"Started Conversion for request Guid: {retArgs.RequestID}");

            string fileExtension = System.IO.Path.GetExtension(fileName);
            
            string conversionSource = $"{workingDir}{fileName}";

            System.IO.File.Copy($"{inputDir}{fileName}", conversionSource);		

            retArgs = ConvertDocument(conversionSource, keywordsJSON, application, retArgs.RequestID);

            return retArgs;        
        } 

        public EngineReturnArgs ConvertDocumentFromBytes(byte[] bytes, string keywordsJSON, string application, string fileType)
        {
            EngineReturnArgs retArgs = new EngineReturnArgs();

            retArgs.RequestID =  Guid.NewGuid();  

            string conversionSource = $"{workingDir}{retArgs.RequestID}.{fileType}";

            File.WriteAllBytes(conversionSource, bytes);
            
            retArgs = ConvertDocument(conversionSource, keywordsJSON, application, retArgs.RequestID);

            return retArgs;
        } 

        private EngineReturnArgs ConvertDocument(string conversionSource, string keywordsJSON, 
                                                    string application, Guid requestGuid)
        {   
			EngineReturnArgs retArgs = new EngineReturnArgs();

            retArgs.RequestID = requestGuid;

            string fileType = System.IO.Path.GetExtension(conversionSource).Replace(".","");

            switch(fileType.ToLower())
            {
                case "pdf":

                    Engine.PDFToText pdfEngine = new Engine.PDFToText();

                    Log.Debug($"Calling convert for pdf: {conversionSource}");

                    retArgs = pdfEngine.ConvertPDFToText(conversionSource,retArgs.RequestID);

                    Log.Debug($"Convert pdf finished");	
            
                    break;

                case "doc":

                    break;

                case "docx":

                    Log.Debug($"Calling convert for docx: {conversionSource}");

                    Engine.WordToText wordEngine = new Engine.WordToText();

                    retArgs = wordEngine.ConvertWordToText(conversionSource,retArgs.RequestID);

                    Log.Debug($"Convert docx finished");	

                    break;

                case "xls":

                    break;

                case "xlsx":
                    
                    Log.Debug($"Calling convert for xlsx: {conversionSource}");

                    Engine.ExcelToText excelEngine = new Engine.ExcelToText();

                    retArgs = excelEngine.ConvertExcelToText(conversionSource,retArgs.RequestID);

                    Log.Debug($"Convert xlsx finished");	

                    break;

                default:

                    Log.Debug($"File type not explicitly handled so calling convert for text: {conversionSource}");

                    Engine.TextToText textEngine = new Engine.TextToText();

                    retArgs = textEngine.ConvertTextToText(conversionSource,retArgs.RequestID);

                    Log.Debug($"Convert default finished");	

                    break;
            }

            if (settings.DeleteWorkingFiles)
            {
                System.IO.File.Delete(conversionSource);
                System.IO.File.Delete(conversionSource.Replace($".{fileType}", ".txt"));
            }

            retArgs.FileName = Path.GetFileName(conversionSource);

            if (retArgs.Success)
            {
                Log.Debug($"Initial Content: {retArgs.DocumentContent}");

                TextProcessorEngine textEngine = new TextProcessorEngine(cache, settings);

                ResultEntity procResult = textEngine.ProcessDocumentContent(retArgs.DocumentContent, keywordsJSON, retArgs.RequestID.ToString(), application, conversionSource);

                if (procResult.Success)
                {   
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                   
                    retArgs.ParsedContent = JsonSerializer.Serialize(procResult, options);                   
                }

                retArgs.DoFormula = procResult.DoFormula;
                retArgs.FileName = Path.GetFileName(conversionSource);
            }

            Log.Debug($"Finished Conversion for request Guid: {retArgs.RequestID}");

            return retArgs;
        }   
    }
}