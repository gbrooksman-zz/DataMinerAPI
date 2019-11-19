using System.Collections.Generic;

namespace DataMinerAPI.Models
{

	public class SearchSet
	{
		public SearchSet()
		{
			
		}

		public bool DoFormula { get; set; }

		public int TotalScore { get; set; }

		public List<SearchTerm> SearchTerms { get; set; }
	}


	/// <summary>
	///
	/// </summary>
	public class SearchTerm
	{
		/// <summary>
		///
		/// </summary>
		public string Item { get; set; }

		/// <summary>
		///
		/// </summary>
		public string Hint { get; set; }

		/// <summary>
		///
		/// </summary>
		public string DataCode { get; set; }

		/// <summary>
		///
		/// </summary>
		public string Section { get; set; }

		/// <summary>
		///
		/// </summary>
		public int Score { get; set; }

		/// <summary>
		///
		/// </summary>
		public string Result { get; set; }

		public List<string> Synonyms { get; set; }
	}	

}
