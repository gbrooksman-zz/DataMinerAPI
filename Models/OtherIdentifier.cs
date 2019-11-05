using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMinerAPI.Models
{
	/// <summary>
	///    various other identifiers  from xml file
	/// </summary>
	public class OtherIdentifier
	{

		/// <summary>
		/// default constructor
		/// </summary>
		public OtherIdentifier()
		{

		}


		/// <summary>
		///  parent node name
		/// </summary>
		public string Parent { get; set; }


		/// <summary>
		///  node value
		/// </summary>
		public string Id { get; set; }

	}
}
