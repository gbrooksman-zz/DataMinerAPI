using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using DataMinerAPI.Engine;
using DataMinerAPI.Models;


namespace DataMinerAPI.Controllers
{
	/// <summary>
	///
	/// </summary>
	[Produces("application/json")]
	[Route("api/TextProcessor")]
	public class TextProcessorController : Controller
	{
		private readonly IMemoryCache cache;
		private readonly IConfiguration config;
		private readonly AppOptions options;

		/// <summary>
		///
		/// </summary>
		/// <param name="_cache"></param>
		/// <param name="_config"></param>
		public TextProcessorController(IMemoryCache _cache, IConfiguration _config, IOptions<AppOptions> _options)
		{
			cache = _cache;
			config = _config;
			options = _options.Value;
		}


		/// <summary>
		///  Get a unique result entity
		/// </summary>
		/// <param name="requestGuid">A Guid representing a unique request to the srevice</param>
		/// <param name="application">Name of the aplication making requests</param>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(typeof(ResultEntity), 200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(500)]
		[Route("api/GetResult")]
		public IActionResult GetResult(string requestGuid, string application)
		{
			ResultEntity entity = new ResultEntity();

			if (string.IsNullOrEmpty(requestGuid))
			{
				return BadRequest("requestGuid argument cannot be blank");
			}

			if (string.IsNullOrEmpty(application))
			{
				return BadRequest("application argument cannot be blank");
			}

			if (Guid.Parse(requestGuid) == Guid.Empty)
			{
				return BadRequest("requestGuid argument cannot be an empty guid");
			}

			try
			{
				StorageEngine storageEngine = new StorageEngine(config);

				entity = storageEngine.GetEntityFromAzure(requestGuid, application);

				if (string.IsNullOrEmpty(entity.ETag))
				{
					return NotFound();
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Cannot fetch entity result data: {ex.StackTrace}");
			}

			return Ok(entity);

		}


		/// <summary>
		///	Get a list of result items for the given application
		/// </summary>
		/// <param name="application">Name of the aplication making requests</param>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(typeof(List<ResultEntity>), 200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(500)]
		[Route("api/GetResultList")]
		public ActionResult<List<ResultEntity>> GetResultList(string application)
		{
			List<ResultEntity> entities = new List<ResultEntity>();

			if (string.IsNullOrEmpty(application))
			{
				return BadRequest("application argument cannot be blank");
			}

			try
			{
				StorageEngine storageEngine = new StorageEngine(config);

				entities = storageEngine.GetEntityListFromAzure(application);

			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Cannot fetch entity list data: {ex.StackTrace}");
			}

			return Ok(entities);

		}

		/// <summary>
		///	Get a count of result items for the given application
		/// </summary>
		/// <param name="application">Name of the aplication making requests</param>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(typeof(List<ResultEntity>), 200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(500)]
		[Route("api/GetCount")]
		public ActionResult<List<ResultEntity>> GeCount(string application)
		{
			int count = 0;

			if (string.IsNullOrEmpty(application))
			{
				return BadRequest("application argument cannot be blank");
			}

			try
			{
				StorageEngine storageEngine = new StorageEngine(config);

				count = storageEngine.GetCountFromAzure(application);

			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Cannot fetch entity list data: {ex.StackTrace}");
			}

			return Ok(count);

		}


		/// <summary>
		///		Process content and return results
		/// </summary>
		/// <param name="requestGuid"></param>
		/// <param name="keywordJson"></param>
		/// <param name="application"></param>
		/// <returns></returns>

#pragma warning disable SG0016 // Controller method is vulnerable to CSRF
		[ProducesResponseType(typeof(IActionResult), 200)]
		[HttpPost]
		public IActionResult Post(string requestGuid, string keywordJson, string application)
		{
			TextProcessorEngine engine = new TextProcessorEngine(cache, config,options);
			TextProcessorReturnArgs retArgs = new TextProcessorReturnArgs();
			List<string> searchTerms = new List<string>();

			try
			{
				Log.Information($"Started Request Guid: {requestGuid} ");

				int ibyteLength = (int)Request.ContentLength.GetValueOrDefault();

				byte[] bytes = new byte[ibyteLength];

				Request.Body.ReadAsync(bytes, 0, ibyteLength);

				string textContent = System.Text.Encoding.UTF8.GetString(bytes);

				Log.Information(textContent);

				retArgs.Content = keywordJson;

				ResultEntity searchResults = engine.ProcessContent(textContent, keywordJson, requestGuid, application);

				retArgs.Message = searchResults.Score.ToString();

				retArgs.Content = JsonConvert.SerializeObject(searchResults);

			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					Success = false,
					Message = ex.Message,
					Content = "Could not process request",
					Guid = Guid.Empty.ToString()
				});
			}
			finally
			{

			}

			return Ok(new
			{
				Success = true,
				Message = retArgs.Message,
				Content = retArgs.Content,
				Guid = requestGuid
			});

		}
#pragma warning restore SG0016 // Controller method is vulnerable to CSRF


	}
}