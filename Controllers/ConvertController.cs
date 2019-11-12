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

		[HttpPost]
		[Route("Test")]		
		public IActionResult Test(string tempValue)
		{
			Log.Information($"Started Test action");

			IActionResult res = Ok();

			string fileName = @"files/2.pdf";
			
			try
			{

				res = this.Post(fileName);

			}
			catch(Exception ex)
			{
				Log.Error(" in Test Method", ex);
			}

			return res;
		}


		[HttpPost]		
		public IActionResult Post(string fileName)
		{
			Guid requestGuid = Guid.NewGuid();

			EngineReturnArgs retArgs = new EngineReturnArgs();

			string fileType = this.GetFileTypeFromName(fileName);

			try
			{
				Log.Information($"Started Request Guid: {requestGuid}");

				//byte[] bytes = System.IO.File.ReadAllBytes(fileName);
				
				//int ibyteLength = bytes.Length;

				//int ibyteLength = (int)Request.ContentLength.GetValueOrDefault();

				//byte[] bytes = new byte[ibyteLength];


				//Request.Body.ReadAsync(bytes, 0, ibyteLength);

				switch(fileType.ToLower())
				{
					case "pdf":

						Engine.PDFToText txtE = new Engine.PDFToText();

						Log.Information($"Calling convert for pdf ");

						retArgs = txtE.ConvertTextFromPDF(fileName,requestGuid);

						Log.Information($"Convert finished");	
				
						break;

					case "doc":

						break;

					case "docx":

					case "xls":

						break;

					case "xlsx":

						break;


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