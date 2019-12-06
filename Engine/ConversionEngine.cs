using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using DataMinerAPI.Models;
using System.Text.Json;

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

        public EngineReturnArgs ConvertDocument(string fileName, string keywordsJSON, string application)
        {   
			EngineReturnArgs retArgs = new EngineReturnArgs();

            retArgs.RequestID =  Guid.NewGuid();  
            
            string fileType = this.GetFileTypeFromName(fileName);

            Log.Debug($"Started Conversion for request Guid: {retArgs.RequestID}");

            string fileExtension = System.IO.Path.GetExtension(fileName);

            string conversionSource = $"{workingDir}{retArgs.RequestID}{fileExtension}";

            System.IO.File.Copy($"{inputDir}{fileName}", conversionSource);		

            switch(fileType.ToLower())
            {
                case "pdf":

                    Engine.PDFToText pdfEngine = new Engine.PDFToText();

                    Log.Debug($"Calling convert for pdf: {retArgs.RequestID}");

                    retArgs = pdfEngine.ConvertPDFToText(conversionSource,retArgs.RequestID, fileExtension);

                    Log.Debug($"Convert pdf finished");	
            
                    break;

                case "doc":

                    break;

                case "docx":

                    Log.Debug($"Calling convert for docx: {retArgs.RequestID}");

                    Engine.WordToText wordEngine = new Engine.WordToText();

                    retArgs = wordEngine.ConvertWordToText(conversionSource,retArgs.RequestID, fileExtension);

                    Log.Debug($"Convert docx finished");	

                    break;

                case "xls":

                    break;

                case "xlsx":
                    
                    Log.Debug($"Calling convert for xlsx: {retArgs.RequestID}");

                    Engine.ExcelToText excelEngine = new Engine.ExcelToText();

                    retArgs = excelEngine.ConvertExcelToText(conversionSource,retArgs.RequestID, fileExtension);

                    Log.Debug($"Convert xlsx finished");	

                    break;

                default:

                    Log.Debug($"File type not explicitly handled so calling convert for text: {retArgs.RequestID}");

                    Engine.TextToText textEngine = new Engine.TextToText();

                    retArgs = textEngine.ConvertTextToText(conversionSource,retArgs.RequestID, fileExtension);

                    Log.Debug($"Convert default finished");	

                    break;
            }

            if (settings.DeleteWorkingFiles)
            {
                System.IO.File.Delete(conversionSource);
                System.IO.File.Delete(conversionSource.Replace(fileType, "txt"));
            }

            retArgs.FileName = fileName; 

            if (retArgs.Success)
            {
                Log.Debug($"Initial Content: {retArgs.DocumentContent}");

                TextProcessorEngine textEngine = new TextProcessorEngine(cache, settings);

                ResultEntity procResult = textEngine.ProcessDocumentContent(retArgs.DocumentContent, keywordsJSON, retArgs.RequestID.ToString(), application, fileName);

                if (procResult.Success)
                {   
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                   
                    retArgs.ParsedContent = JsonSerializer.Serialize(procResult, options);
                   
                }

                retArgs.DoFormula = procResult.DoFormula;
                retArgs.FileName = fileName;
            }

            Log.Debug($"Finished Conversion for request Guid: {retArgs.RequestID}");

            return retArgs;
        } 

        private string GetFileTypeFromName(string fileName)
		{
			return System.IO.Path.GetExtension(fileName).Replace(".","");
		}      

    }

}