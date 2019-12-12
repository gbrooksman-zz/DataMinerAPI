using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace DataMinerAPI.Models
{	
	public class SearchCriteria
	{
		//this class is populated from keywords.json serialization
		// so it is the object that contains all of the initial search criteria
		public SearchCriteria()
		{
			
		}

		/// <summary>
		/// if running in batchmode, the name of the file being processed
		/// </summary>
		/// <value></value>
		public string FileName { get; set; }

		/// <summary>
		/// Should the search process look for a formulation?
		/// </summary>
		/// <value></value>
		public bool DoFormula { get; set; }

		/// <summary>
		/// a value from 0 to the maxformulascore setting specified in appsettings.json 
		/// that indicates the number and confidence of formula items identified in the document
		/// </summary>
		/// <value></value>
		public int FormulaScore { get; set; }
		
		/// <summary>
		/// a value from 0 to the maxattributescore setting specified in appsettings.json 
		/// that indicates the number and confidence of keyword search items identified in the document
		/// </summary>
		/// <value></value>
		public int DocItemScore { get; set; }
		public List<DocItem> DocItems { get; set; }
		public List<FormulaItem> FormulaItems { get; set; }
	}


	/// <summary>
	///	this entity is used to define a particular item being searched for.
	/// </summary>
	public class DocItem
	{
		/// <summary>
		/// informational only, not used in the search process
		/// </summary>
		/// <value></value>
		public string Description { get; set; }

		/// <summary>
		/// helps the search parser look for numeric, text or boolean data types
		/// </summary>
		/// <value></value>
		public string Hint { get; set; }
		public string DataCode { get; set; }
		
		/// <summary>
		/// the section of the SDS to focus the search on for this item.
		/// should be a value from 1 to 16.!-- Other values will be ignored
		/// </summary>
		/// <value></value>
		public string Section { get; set; }
		
		/// <summary>
		/// a value from 0 to 10, the higher the score the higher the confidence in the result
		/// </summary>
		/// <value></value>
		public int Score { get; set; }
		public string Result { get; set; }

		/// <summary>
		/// a list of alternate identifiers for the search term
		/// </summary>
		/// <value></value>
		public List<string> Terms { get; set; }

		/// <summary>
		/// a list of alternate idenitifiers for the keyword(s)
		/// </summary>
		/// <value></value>
		public List<string> ParentTerms { get; set; }
	}	
}
