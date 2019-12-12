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

		[ApiExplorerSettings(IgnoreApi=true)]
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

		[ApiExplorerSettings(IgnoreApi=true)]
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

		[ApiExplorerSettings(IgnoreApi=true)]
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

		[ApiExplorerSettings(IgnoreApi=true)]
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

		[ApiExplorerSettings(IgnoreApi=true)]
		[HttpPost]
		[Route("ConvertFile")]	
		[ProducesResponseType(StatusCodes.Status200OK)]	
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		
		public IActionResult ConvertFile(string fileName, string keywordsJSON, string application)
		{	
			ResponseEntity respEntity = new ResponseEntity();

			try
			{
				ConversionEngine conversionEngine = new ConversionEngine(cache, settings);
				respEntity = conversionEngine.ConvertDocumentFromFile(fileName, keywordsJSON, application);	

			if (respEntity.Success)
			{
				return Ok(new
				{
					filename = respEntity.FileName,
					success = true,
					message = respEntity.Message,
					documentcontent = respEntity.DocumentContent,
					parsedcontent = respEntity.ParsedContent,
					requestid = respEntity.RequestID.ToString(),
					doformula = respEntity.DoFormula
				});
			}
			else
			{
				//this isn't quite the right response... but for now, ok
				return NotFound(new ProblemDetails()
				{
					Title = "Not found in ConvertFile Method",
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
					Title = "Error in ConvertFile Method",
					Status = (int) HttpStatusCode.BadRequest,
					Detail = ex.Message,
					Type = "/api/problem/bad-doc-type",					
					Instance = HttpContext.Request.Path
				});
			}			
		}

		/// <summary>
		/// This method converts a file into text and searches for a set of keywords.
		/// The request body must contain a byte array that is the document content.
		/// </summary>
		/// <param name="keywordsFile">The file name of a keywords file to use as search criteria.!-- The keywords file must be present in the files/keywords folder of this service</param>
		/// <param name="application">The application calling this service</param>
		/// <param name="fileType">The extension of file to be seached (omit the period)</param>
		/// <remarks>
		/// Sample request:
		/// POST: ConvertAndSearch
		/// {
		/// 	"keywords": "keywords.json"	,
		/// 	"application": "SCN",
		/// 	"filetype: "pdf"
		/// }
		/// </remarks>
		/// <returns></returns>
		/// <response code="200">a populated entity containing the search results</response>
		/// <response code="400">the document could not be parsed and/or searched.Most likely the byte array is corrupt	</response>
		/// <response code="500">an unknown error occurred, the response body may contain more information</response>
		[HttpPost]
		[Route("ConvertAndSearch")]	
		[ProducesResponseType(StatusCodes.Status200OK)]	
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[Produces("application/json", Type = typeof(SearchResults))]
		public IActionResult ConvertAndSearch(string keywordsFile, string application, string fileType)
		{				
			ResponseEntity respEntity = new ResponseEntity();

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

				respEntity = conversionEngine.ConvertDocumentFromBytes(bytes, keywordsJSON, application, fileType);	

			if (respEntity.Success)
			{
				return Ok(new
				{
					filename = respEntity.FileName,
					success = true,
					message = respEntity.Message,
					documentcontent = respEntity.DocumentContent,
					parsedcontent = respEntity.ParsedContent,
					requestid = respEntity.RequestID.ToString(),
					doformula = respEntity.DoFormula
				});
			}
			else
			{
				return BadRequest(new ProblemDetails()
				{
					Title = "Error in ConvertAndSearch Method",
					Status = (int) HttpStatusCode.BadRequest,
					Detail = "Could not convert byte array",
					Type = "/api/problem/bad-doc-type",					
					Instance = HttpContext.Request.Path
				});
			}
			}
			catch (Exception ex)
			{	
				var responseObject = new ProblemDetails()
				{
					Title = "Error in ConvertAndSearch Method",
					Status = (int) HttpStatusCode.BadRequest,
					Detail = ex.Message,
					Type = "/api/problem/general-failure",					
					Instance = HttpContext.Request.Path
				};

				return StatusCode(StatusCodes.Status500InternalServerError, responseObject);
			}			
		}
	}
}