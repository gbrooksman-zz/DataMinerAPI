using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using System;

namespace DataMinerAPI.Controllers
{
    [ApiController]
    [Produces("application/json")]
	[Route("api/problem")]
    [ApiExplorerSettings(IgnoreApi=true)]
    public class ProblemController : ControllerBase
    {
        [Route("bad-doc-type")]
        [HttpGet]
        public IActionResult BadDocType()
        {
            return Ok("A document type was provided that this service does not support");

        }

        [Route("missing-keywords")]
        [HttpGet]
        public IActionResult MissingKeywords()
        {
            return Ok("Keywords were not supplied so the conversion cannot proceed");
        }

        [Route("general-failure")]
        [HttpGet]
        public IActionResult GeneralFailure()
        {
            return Ok("an unknown error has prevented the service from completing");
        }
        
        [Route("bad-convert-batch")]
        [HttpGet]
        public IActionResult BadConvertBatch()
        {
            return Ok("an error has prevented the service from completing a batch conversion process");
        }

    }
}