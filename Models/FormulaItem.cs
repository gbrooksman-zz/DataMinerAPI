using Microsoft.AspNetCore.Mvc;

namespace DataMinerAPI.Models
{
	/// <summary>
	/// a chemical substance identified by the search process
	/// </summary>

	public class FormulaItem
	{
		public FormulaItem() { }

		public string CASNumber { get; set; }
	 
		public string ChemName { get; set; }

		public string Percent { get; set; }

		public string OtherInfo { get; set; }

		/// <summary>
		/// a value from 0 to 10, the higher the score the higher the confidence in the result
		/// </summary>
		/// <value></value>
		public int Score { get; set; }

	}
}
