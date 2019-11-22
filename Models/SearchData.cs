using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataMinerAPI.Models
{

	public class SearchData
	{
		public SearchData()
		{
			
		}

        public List<Component> KnownChemicals {get; set;}
		public List<SectionHeader> SectionHeaders {get; set;}
		public List<OtherIdentifier> OtherIdentifiers {get; set;}
        public List<Section> SectionList {get; set;}


    }
}
