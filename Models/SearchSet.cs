using System.Collections.Generic;

namespace DataMinerAPI.Models
{	
	public class SearchSet
	{
		//this class is populated from keyuwords.json serialization
		public SearchSet()
		{
			
		}

		public string FileName { get; set; }
		public bool DoFormula { get; set; }
		public int FormulaScore { get; set; }
		public int DocItemScore { get; set; }
		public List<DocItem> DocItems { get; set; }
		public List<FormulaItem> FormulaItems { get; set; }
	}


	/// <summary>
	///
	/// </summary>
	public class DocItem
	{
		public string Description { get; set; }
		public string Hint { get; set; }
		public string DataCode { get; set; }
		public string Section { get; set; }
		public int Score { get; set; }
		public string Result { get; set; }
		public List<string> Terms { get; set; }
		public List<string> ParentTerms { get; set; }
	}	
}
