using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace DataMinerAPI.Models
{
	/// <summary>
	/// This entity is used by the service internally after the document has been parsed to accumulate the results 
	/// of the keyword searching process. instances of this class are aggregated into 
	/// the responseentity returned by the controller.
	/// </summary>

	public class SearchResults 
	{	
		public SearchResults() { }

		public string RequestGuid { get; set; }

		public string Application { get; set; }

		public string Content { get; set; }

		public List<DocItem> DocItems { get; set; }

		public List<FormulaItem> FormulaItems { get; set; }
	
		public int FormulaScore { get; set; }

		public int DocItemScore { get; set; }

		public List<string> Messages { get; set; }

		public DateTimeOffset DateStamp { get; set; }

		[XmlIgnore]
		public Exception AppException { get; set; }

		public bool Success { get; set; }

		public string FileName { get; set; }

		public bool DoFormula { get; set; }
	}
}
