using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataMinerAPI.Models
{
	/// <summary>
	/// 
	/// </summary>
	public class ResultEntity : TableEntity
	{		
		public ResultEntity(string reqGuid, string partitionKey)
		{
			RequestGuid = reqGuid;
			RowKey = reqGuid;
			PartitionKey = partitionKey;
			Application = partitionKey;
		}

		public ResultEntity() { }

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
