using Microsoft.AspNetCore.Mvc;
using DataMinerAPI.Engine;
using System;
using Serilog;

namespace DataMinerAPI.Controllers
{
	[Produces("application/json")]
	[Route("api/convert")]

	public class ConvertController : Controller
	{
		private readonly ServiceSettings settings;
		public ConvertController(ServiceSettings _settings)
		{
			settings = _settings;
		}

		[HttpPost]
		[Route("TestPDF")]		
		public IActionResult TestPDF(string tempValue)
		{
			Log.Debug(" in TestPDF method ");

			IActionResult res = Ok();

			string fileName = "1.pdf";
			
			try
			{
				res = this.Post(fileName);
			}
			catch(Exception ex)
			{
				Log.Error(" in TestPDF Method", ex);
				res = BadRequest();
			}

			return res;
		}


		[HttpPost]
		[Route("TestWord")]		
		public IActionResult TestWord(string tempValue)
		{
			Log.Debug(" in TestWord method ");

			IActionResult res = Ok();

			string fileName = "1.docx";
			
			try
			{
				res = this.Post(fileName);

			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error(" in TestWord Method", ex);
			}

			return res;
		}

		[HttpPost]
		[Route("TestExcel")]		
		public IActionResult TestExcel(string tempValue)
		{
			Log.Debug(" in Testexcel method ");

			IActionResult res = Ok();

			string fileName = "2.xlsx";
			
			try
			{
				res = this.Post(fileName);
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error(" in TestExcel Method", ex);				
			}

			return res;
		}

		[HttpPost]
		[Route("TestText")]		
		public IActionResult TestText(string tempValue)
		{
			Log.Debug(" in TestText method ");

			IActionResult res = Ok();

			string fileName = "1.txt";
			
			try
			{
				res = this.Post(fileName);
			}
			catch(Exception ex)
			{
				res = BadRequest();
				Log.Error(" in TestText Method", ex);
			}

			return res;
		}

		[HttpPost]		
		public IActionResult Post(string fileName)
		{			
			string inputDir = settings.FilesFolder;
			string workingDir = settings.WorkingFolder;

			Guid requestGuid = Guid.NewGuid();

			EngineReturnArgs retArgs = new EngineReturnArgs();

			string fileType = this.GetFileTypeFromName(fileName);

			try
			{
			//	Log.Information($"Started Conversion for request Guid: {requestGuid}");

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

					//	Log.Information($"Calling convert for pdf ");

						retArgs = pdfEngine.ConvertPDFToText(conversionSource,requestGuid, fileExtension);

				//		Log.Information($"Convert finished");	
				
						break;

					case "doc":

						break;

					case "docx":

					//	Log.Information($"Calling convert for docx");

							Engine.WordToText wordEngine = new Engine.WordToText();

							retArgs = wordEngine.ConvertWordToText(conversionSource,requestGuid, fileExtension);

					//		Log.Information($"Convert finished");	

							break;

					case "xls":

						break;

					case "xlsx":
						
						Log.Information($"Calling convert for docx");

						Engine.ExcelToText excelEngine = new Engine.ExcelToText();

						retArgs = excelEngine.ConvertExcelToText(conversionSource,requestGuid, fileExtension);

						Log.Information($"Convert finished");	

						break;

					default:

						Log.Information($"File type not explicitly handled so calling convert for text");

						Engine.TextToText textEngine = new Engine.TextToText();

						retArgs = textEngine.ConvertTextToText(conversionSource,requestGuid, fileExtension);

						Log.Information($"Convert finished");	

						break;
				}

				if (settings.DeleteWorkingFiles)
				{
					System.IO.File.Delete(conversionSource);
					System.IO.File.Delete(conversionSource.Replace(fileType, "txt"));
				}

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