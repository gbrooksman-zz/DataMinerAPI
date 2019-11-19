using Microsoft.AspNetCore.Mvc;
using DataMinerAPI.Engine;
using System;
using Serilog;
using Microsoft.Extensions.Caching.Memory;

namespace DataMinerAPI.Controllers
{
	[Produces("application/json")]
	[Route("api/convert")]

	public class ConvertController : Controller
	{
		private readonly ServiceSettings settings;
		private readonly IMemoryCache cache;

		public ConvertController(IMemoryCache _cache, ServiceSettings _settings)
		{
			settings = _settings;
		}

		private string GetSampleKeywords()
		{
			return System.IO.File.ReadAllText(@"/files/keywords.xml");
		}

		[HttpPost]
		[Route("TestPDF")]		
		public IActionResult TestPDF(string tempValue)
		{
			Log.Debug("In TestPDF method ");

			IActionResult res = Ok();

			string fileName = "1.pdf";
			
			try
			{
				res = this.Post(fileName, GetSampleKeywords(), "Test");
			}
			catch(Exception ex)
			{
				Log.Error("TestPDF Method", ex);
				res = BadRequest();
			}

			return res;
		}


		[HttpPost]
		[Route("TestWord")]		
		public IActionResult TestWord(string tempValue)
		{
			Log.Debug("In TestWord method ");

			IActionResult res = Ok();

			string fileName = "1.docx";
			
			try
			{
				res = this.Post(fileName, GetSampleKeywords(), "Test");

			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error("TestWord Method", ex);
			}

			return res;
		}

		[HttpPost]
		[Route("TestExcel")]		
		public IActionResult TestExcel(string tempValue)
		{
			Log.Debug("In TestExcel method ");

			IActionResult res = Ok();

			string fileName = "2.xlsx";
			
			try
			{
				res = this.Post(fileName, GetSampleKeywords(), "Test");
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error("TestExcel Method", ex);				
			}

			return res;
		}

		[HttpPost]
		[Route("TestText")]		
		public IActionResult TestText(string tempValue)
		{
			Log.Debug("In TestText method ");

			IActionResult res = Ok();

			string fileName = "1.txt";
			
			try
			{
				res = this.Post(fileName, GetSampleKeywords(), "Test");
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error("TestText Method", ex);
			}

			return res;
		}

		[HttpPost]		
		public IActionResult Post(string fileName, string keyWordsXML, string application)
		{			
			string inputDir = settings.FilesFolder;
			string workingDir = settings.WorkingFolder;

			Guid requestGuid = Guid.NewGuid();

			EngineReturnArgs retArgs = new EngineReturnArgs();

			string fileType = this.GetFileTypeFromName(fileName);

			try
			{
				Log.Debug($"Started Conversion for request Guid: {requestGuid}");

				string fileExtension = System.IO.Path.GetExtension(fileName);

				string conversionSource = $"{workingDir}{requestGuid}{fileExtension}";

				System.IO.File.Copy($"{inputDir}{fileName}", conversionSource);		

				//byte[] bytes = System.IO.File.ReadAllBytes(fileName);
				
				//int ibyteLength = bytes.Length;

				//int ibyteLength = (int)Request.ContentLength.GetValueOrDefault();

				//byte[] bytes = new byte[ibyteLength];

				//Request.Body.ReadAsync(bytes, 0, ibyteLength);

				switch(fileType.ToLower())
				{
					case "pdf":

						Engine.PDFToText pdfEngine = new Engine.PDFToText();

						Log.Debug($"Calling convert for pdf: {requestGuid}");

						retArgs = pdfEngine.ConvertPDFToText(conversionSource,requestGuid, fileExtension);

						Log.Debug($"Convert pdf finished");	
				
						break;

					case "doc":

						break;

					case "docx":

						Log.Debug($"Calling convert for docx: {requestGuid}");

						Engine.WordToText wordEngine = new Engine.WordToText();

						retArgs = wordEngine.ConvertWordToText(conversionSource,requestGuid, fileExtension);

						Log.Debug($"Convert docx finished");	

						break;

					case "xls":

						break;

					case "xlsx":
						
						Log.Debug($"Calling convert for xlsx: {requestGuid}");

						Engine.ExcelToText excelEngine = new Engine.ExcelToText();

						retArgs = excelEngine.ConvertExcelToText(conversionSource,requestGuid, fileExtension);

						Log.Debug($"Convert xlsx finished");	

						break;

					default:

						Log.Debug($"File type not explicitly handled so calling convert for text: {requestGuid}");

						Engine.TextToText textEngine = new Engine.TextToText();

						retArgs = textEngine.ConvertTextToText(conversionSource,requestGuid, fileExtension);

						Log.Debug($"Convert default finished");	

						break;
				}

				if (settings.DeleteWorkingFiles)
				{
					System.IO.File.Delete(conversionSource);
					System.IO.File.Delete(conversionSource.Replace(fileType, "txt"));
				}

				if (retArgs.Success)
				{
					TextProcessorEngine textEngine = new TextProcessorEngine(cache, settings);

					textEngine.ProcessContent(retArgs.Content, keyWordsXML, requestGuid.ToString(), application);

				}

				Log.Debug($"Content: {retArgs.Content}");

				Log.Debug($"Finished Conversion for request Guid: {requestGuid}");


			}
			catch (Exception ex)
			{
				Log.Error(ex, "Could not convert file");

				return BadRequest(new
				{
					success = false,
					message = ex.Message,
					content = "Could not convert file",
					guid = Guid.Empty.ToString()
				});
			}

			if (retArgs.Success)
			{
				return Ok(new
				{
					success = true,
					message = retArgs.Message,
					content = retArgs.Content,
					guid = requestGuid.ToString()
				});
			}
			else
			{
				return NoContent();  //no result but no exception --???
			}
		}

		private string GetFileTypeFromName(string fileName)
		{
			return System.IO.Path.GetExtension(fileName).Replace(".","");
		}


	}





}