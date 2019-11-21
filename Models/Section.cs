using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataMinerAPI.Models
{
	//   Section headers from xml file - handles the various names/titles for each section	
	public class Section
	{
		public Section()
		{

		}
		public string Number { get; set; }

		public List<string> Content { get; set; }
	}
}