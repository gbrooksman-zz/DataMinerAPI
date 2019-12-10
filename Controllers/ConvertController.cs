using Microsoft.AspNetCore.Mvc;
using DataMinerAPI.Engine;
using System;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using DataMinerAPI.Models;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;

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
			cache = _cache;
		}

		private string GetSampleKeywords()
		{
			return System.IO.File.ReadAllText($"{settings.FilesFolder}keywords/keywords.json");
		}

		[HttpPost]
		[Route("TestPDF")]
		public IActionResult TestPDF()
		{
			Log.Debug("In TestPDF method ");

			IActionResult res = Ok();

			string fileName = "1.pdf";
			
			try
			{				
				res = this.ConvertFile(fileName, GetSampleKeywords(), "Test");
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error("TestWord Method", ex);				
			}

			return res;
		}


		[HttpPost]
		[Route("TestWord")]		
		public IActionResult TestWord()
		{
			Log.Debug("In TestWord method ");

			IActionResult res = Ok();

			string fileName = "1.docx";
			
			try
			{
				res = this.ConvertFile(fileName, GetSampleKeywords(), "Test");

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
		public IActionResult TestExcel()
		{
			Log.Debug("In TestExcel method ");

			IActionResult res = Ok();

			string fileName = "2.xlsx";
			
			try
			{
				res = this.ConvertFile(fileName, GetSampleKeywords(), "Test");
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
		public IActionResult TestText()
		{
			Log.Debug("In TestText method ");

			IActionResult res = Ok();

			string fileName = "1.txt";
			
			try
			{
				res = this.ConvertFile(fileName, GetSampleKeywords(), "Test");
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error("TestText Method", ex);
			}

			return res;
		}

		[HttpPost]
		[Route("TestBatch")]		
		public IActionResult TestBatch()
		{
			Log.Debug("In TestBatch method ");

			IActionResult res = Ok();
			
			try
			{
				res = this.ConvertBatch(GetSampleKeywords(), "Test");
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error("TestBatch Method", ex);
			}

			return res;
		}



		[HttpPost]
		[Route("ConvertFile")]	
		[ProducesResponseType(StatusCodes.Status200OK)]	
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult ConvertFile(string fileName, string keywordsJSON, string application)
		{	
			EngineReturnArgs retArgs = new EngineReturnArgs();

			try
			{
				ConversionEngine conversionEngine = new ConversionEngine(cache, settings);
				retArgs = conversionEngine.ConvertDocumentFromFile(fileName, keywordsJSON, application);	

			if (retArgs.Success)
			{
				return Ok(new
				{
					filename = retArgs.FileName,
					success = true,
					message = retArgs.Message,
					documentcontent = retArgs.DocumentContent,
					parsedcontent = retArgs.ParsedContent,
					guid = retArgs.RequestID.ToString()
				});
			}
			else
			{
				//this isn't quite the right response... but for now, ok
				return NotFound(new ProblemDetails()
				{
					Title = "Not found in Post Method",
					Status = (int) HttpStatusCode.NotFound,
					Detail = "No exception",
					Type = "/api/problem/general-failure",					
					Instance = HttpContext.Request.Path
				});
			}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not convert file: {fileName}");

				return BadRequest(new ProblemDetails()
				{
					Title = "Error in Post Method",
					Status = (int) HttpStatusCode.BadRequest,
					Detail = ex.Message,
					Type = "/api/problem/bad-doc-type",					
					Instance = HttpContext.Request.Path
				});
			}			
		}

		[HttpPost]
		[Route("ConvertBytes")]	
		[ProducesResponseType(StatusCodes.Status200OK)]	
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult ConvertBytes(string keywordsFile, string application, string fileType)
		{				
			EngineReturnArgs retArgs = new EngineReturnArgs();

			try
			{
				ConversionEngine conversionEngine = new ConversionEngine(cache, settings);

				int ibyteLength = (int)Request.ContentLength.GetValueOrDefault();

				byte[] bytes = new byte[ibyteLength];

				Request.Body.ReadAsync(bytes, 0, ibyteLength);

				string keywordsJSON = "keywords.json";

				if (!string.IsNullOrEmpty(keywordsFile))				
				{
					keywordsJSON = System.IO.File.ReadAllText($"{settings.KeywordsFolder}{keywordsFile}");
				}

				retArgs = conversionEngine.ConvertDocumentFromBytes(bytes, keywordsJSON, application, fileType);	

			if (retArgs.Success)
			{
				return Ok(new
				{
					filename = retArgs.FileName,
					success = true,
					message = retArgs.Message,
					documentcontent = retArgs.DocumentContent,
					parsedcontent = retArgs.ParsedContent,
					guid = retArgs.RequestID.ToString()
				});
			}
			else
			{
				//this isn't quite the right response... but for now, ok
				return NotFound(new ProblemDetails()
				{
					Title = "Not found in Post Method",
					Status = (int) HttpStatusCode.NotFound,
					Detail = "No exception",
					Type = "/api/problem/general-failure",					
					Instance = HttpContext.Request.Path
				});
			}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not convert byte array");

				return BadRequest(new ProblemDetails()
				{
					Title = "Error in Post Method",
					Status = (int) HttpStatusCode.BadRequest,
					Detail = ex.Message,
					Type = "/api/problem/bad-doc-type",					
					Instance = HttpContext.Request.Path
				});
			}			
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]	
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult ConvertBatch(string keywordsJSON, string application)
		{	
			EngineReturnArgs retArgs = new EngineReturnArgs();

			try
			{
				ConversionEngine conversionEngine = new ConversionEngine(cache, settings);

				BatchOutputBuilder batchBuilder = new BatchOutputBuilder(cache, settings);
				
				batchBuilder.InitOutputFiles();

				foreach (string fileName in Directory.GetFiles(settings.FilesFolder))
				{	
					string fileNameOnly = Path.GetFileName(fileName);	
							
					retArgs = conversionEngine.ConvertDocumentFromFile(fileNameOnly, keywordsJSON, application);

					if (retArgs.Success)
					{
						batchBuilder.AddBatchItem(retArgs);
					}
				}
			if (retArgs.Success)
			{
				return Ok( new
				{
					success = true,
					message = "Batch process completed. check result_log.txt and batchconvert.xml"
				});
			}
			else
			{
				//this isn't quite the right response... but for now, ok
				return NotFound(new ProblemDetails()
				{
					Title = "Not found in Post Method",
					Status = (int) HttpStatusCode.NotFound,
					Detail = "No exception",
					Type = "/api/problem/general-failure",					
					Instance = HttpContext.Request.Path
				});
			}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Could not convert one or more files: {settings.FilesFolder}");

				return BadRequest(new ProblemDetails()
				{
					Title = "Error in ConvertBatch Method",
					Status = (int) HttpStatusCode.BadRequest,
					Detail = ex.Message,
					Type = "/api/problem/bad-convert-batch",					
					Instance = HttpContext.Request.Path
				});
			}			
		}
	}
}