using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace DataMinerAPI.Models
{
	/// <summary>
	/// 
	/// </summary>
	public class ResultEntity : TableEntity
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="reqGuid"></param>
		/// <param name="partitionKey"></param>
		public ResultEntity(string reqGuid, string partitionKey)
		{
			RequestGuid = reqGuid;
			RowKey = reqGuid;
			PartitionKey = partitionKey;
			Application = partitionKey;
		}


		/// <summary>
		/// 
		/// </summary>
		public ResultEntity() { }

		/// <summary>
		/// 
		/// </summary>
		public string RequestGuid { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Application { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public List<DocItem> DocItems { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public List<FormulaItem> FormulaItems { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int Score { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public List<string> Messages { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTimeOffset DateStamp { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool Success { get; set; }

	}
}
