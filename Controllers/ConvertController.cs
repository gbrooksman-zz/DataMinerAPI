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
		[ValidateAntiForgeryToken] 
		public IActionResult Post()
		{
			Guid requestGuid = Guid.NewGuid();

			EngineReturnArgs retArgs = new EngineReturnArgs();

			try
			{
				Log.Information($"Started Request Guid: {requestGuid}");

				int ibyteLength = (int)Request.ContentLength.GetValueOrDefault();

				byte[] bytes = new byte[ibyteLength];

				Request.Body.ReadAsync(bytes, 0, ibyteLength);

				Engine.PDFToText txtE = new Engine.PDFToText();

				Log.Information($"Calling convert for input length: //{ibyteLength.ToString()}");

				retArgs = txtE.ConvertTextPDF(bytes);

				Log.Information($"Convert finished, result length: //{retArgs.Content.Length} ");	

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
	}
}