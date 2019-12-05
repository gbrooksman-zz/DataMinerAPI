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
			return System.IO.File.ReadAllText(@"files/keywords.json");
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
				res = BadRequest();
				Log.Error("TestWord Method", ex);				
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
		[ProducesResponseType(StatusCodes.Status200OK)]	
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult Post(string fileName, string keywordsJSON, string application)
		{	
			EngineReturnArgs retArgs = new EngineReturnArgs();

			try
			{
				ConversionEngine conversionEngine = new ConversionEngine(cache, settings);
				retArgs = conversionEngine.ConvertDocument(fileName, keywordsJSON, application);	

			if (retArgs.Success)
			{
				return Ok(new
				{
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
	}
}