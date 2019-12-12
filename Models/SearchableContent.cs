using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace DataMinerAPI.Models
{

	/// <summary>
	/// this class holds content that is read from xml files and represents 
	/// known entites in each of the areas.!-- the content is held in xml files
	/// stored in the Data folder of this service.
	/// </summary>
	public class SearchableContent
	{
		public SearchableContent()
		{
			
		}

        public List<Component> KnownChemicals {get; set;}
		public List<SectionHeader> SectionHeaders {get; set;}
		public List<OtherIdentifier> OtherIdentifiers {get; set;}
        public List<Section> SectionList {get; set;}


    }
}
