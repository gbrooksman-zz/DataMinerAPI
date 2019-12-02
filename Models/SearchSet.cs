using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataMinerAPI.Models
{

	public class SearchSet
	{
		public SearchSet()
		{
			
		}

		public bool DoFormula { get; set; }

		public int FormulaScore { get; set; }

		public int DocItemScore { get; set; }
		
		public List<DocItem> DocItems { get; set; }
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

		[XmlArray]
    	[XmlArrayItem(ElementName="Term")]
		public List<string> Terms { get; set; }

		[XmlArray]
    	[XmlArrayItem(ElementName="Term")]
		public List<string> ParentTerms { get; set; }
	}	


	public class Terms 
	{		
		public List<string> Term { get; set; }
	}

	public class ParentTerms 
	{		
		public List<string> Term { get; set; }
	}
}
