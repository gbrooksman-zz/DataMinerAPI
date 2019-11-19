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

		public List<DocItem> DocItems { get; set; }
	}


	/// <summary>
	///
	/// </summary>
	public class DocItem
	{
		/// <summary>
		///
		/// </summary>
		public string Description { get; set; }

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

		public Terms Terms { get; set; }
	}	


	public class Terms 
	{		
		public List<string> Term { get; set; }
	}

}
