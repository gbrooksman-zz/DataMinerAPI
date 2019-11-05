using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMinerAPI.Models
{
	/// <summary>
	///
	/// </summary>
	public class AppOptions
	{

		/// <summary>
		///
		/// </summary>
		public AppOptions()
		{

		}

		/// <summary>
		///
		/// </summary>
		public bool SaveToAzure { get; set; }


		/// <summary>
		///
		/// </summary>
		public bool SaveToLocalSQL { get; set; }


		/// <summary>
		///
		/// </summary>
		public int MaxAttributeScore { get; set; }

		/// <summary>
		///
		/// </summary>
		public int MaxFormulaScore { get; set; }

	}
}
