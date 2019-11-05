using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMinerAPI.Models
{
	/// <summary>
	///   Section headers from xml file - handles the various names/titles for each section
	/// </summary>
	public class SectionHeader
	{

		/// <summary>
		///  default constructor
		/// </summary>
		public SectionHeader()
		{

		}

		/// <summary>
		///  the section number
		/// </summary>
		public string Number { get; set; }


		/// <summary>
		///  the section title
		/// </summary>
		public string Title { get; set; }


	}
}
