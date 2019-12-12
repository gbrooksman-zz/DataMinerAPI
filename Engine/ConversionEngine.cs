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

        public ResponseEntity ConvertDocumentFromFile(string fileName, string keywordsJSON, string application)
        {
            ResponseEntity respEntity = new ResponseEntity();

            respEntity.RequestID =  Guid.NewGuid();              

            Log.Debug($"Started Conversion for request Guid: {respEntity.RequestID}");

            string fileExtension = System.IO.Path.GetExtension(fileName);
            
            string conversionSource = $"{workingDir}{fileName}";

            System.IO.File.Copy($"{inputDir}{fileName}", conversionSource);		

            respEntity = ConvertDocument(conversionSource, keywordsJSON, application, respEntity.RequestID);

            return respEntity;        
        } 

        public ResponseEntity ConvertDocumentFromBytes(byte[] bytes, string keywordsJSON, string application, string fileType)
        {
            ResponseEntity respEntity = new ResponseEntity();

            respEntity.RequestID =  Guid.NewGuid();  

            string conversionSource = $"{workingDir}{respEntity.RequestID}.{fileType}";

            File.WriteAllBytes(conversionSource, bytes);
            
            respEntity = ConvertDocument(conversionSource, keywordsJSON, application, respEntity.RequestID);

            return respEntity;
        } 

        private ResponseEntity ConvertDocument(string conversionSource, string keywordsJSON, 
                                                    string application, Guid requestGuid)
        {   
			ResponseEntity respEntity = new ResponseEntity();

            respEntity.RequestID = requestGuid;

            string fileType = System.IO.Path.GetExtension(conversionSource).Replace(".","");

            switch(fileType.ToLower())
            {
                case "pdf":

                    Engine.PDFToText pdfEngine = new Engine.PDFToText();

                    Log.Debug($"Calling convert for pdf: {conversionSource}");

                    respEntity = pdfEngine.ConvertPDFToText(conversionSource,respEntity.RequestID);

                    Log.Debug($"Convert pdf finished");	
            
                    break;

                case "doc":

                    break;

                case "docx":

                    Log.Debug($"Calling convert for docx: {conversionSource}");

                    Engine.WordToText wordEngine = new Engine.WordToText();

                    respEntity = wordEngine.ConvertWordToText(conversionSource,respEntity.RequestID);

                    Log.Debug($"Convert docx finished");	

                    break;

                case "xls":

                    break;

                case "xlsx":
                    
                    Log.Debug($"Calling convert for xlsx: {conversionSource}");

                    Engine.ExcelToText excelEngine = new Engine.ExcelToText();

                    respEntity = excelEngine.ConvertExcelToText(conversionSource,respEntity.RequestID);

                    Log.Debug($"Convert xlsx finished");	

                    break;

                default:

                    Log.Debug($"File type not explicitly handled so calling convert for text: {conversionSource}");

                    Engine.TextToText textEngine = new Engine.TextToText();

                    respEntity = textEngine.ConvertTextToText(conversionSource,respEntity.RequestID);

                    Log.Debug($"Convert default finished");	

                    break;
            }

            if (settings.DeleteWorkingFiles)
            {
                System.IO.File.Delete(conversionSource);
                System.IO.File.Delete(conversionSource.Replace($".{fileType}", ".txt"));
            }

            respEntity.FileName = Path.GetFileName(conversionSource);

            if (respEntity.Success)
            {
                Log.Debug($"Initial Content: {respEntity.DocumentContent}");

                TextProcessorEngine textEngine = new TextProcessorEngine(cache, settings);

                SearchResults procResult = textEngine.ProcessDocumentContent(respEntity.DocumentContent, keywordsJSON, respEntity.RequestID.ToString(), application, conversionSource);

                if (procResult.Success)
                {   
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                   
                    respEntity.ParsedContent = JsonSerializer.Serialize(procResult, options);                   
                }

                respEntity.DoFormula = procResult.DoFormula;
                respEntity.FileName = Path.GetFileName(conversionSource);
            }

            Log.Debug($"Finished Conversion for request Guid: {respEntity.RequestID}");

            return respEntity;
        }   
    }
}