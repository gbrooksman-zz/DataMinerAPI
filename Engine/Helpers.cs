
using System;
using System.IO;
using System.Text;
using System.Linq;
using Serilog;
using DataMinerAPI.Models;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace DataMinerAPI.Engine
{
    public class Helpers
	{
        private readonly IMemoryCache cache;
		private readonly ServiceSettings settings;
		
        public Helpers(IMemoryCache _cache, ServiceSettings _settings)
        {
            cache = _cache;
			settings = _settings;
        }

		public List<string> GetSection(List<string> textlines, string startSection, string stopSection, SearchData searchData)
		{	
			//sectionHeaders string are already lower case

			List<string> frag = new List<string>();

			//if we already parsed the section use it again
			if (searchData.SectionList.Any(s => s.Number == startSection))
			{
				frag = searchData.SectionList.Where( s => s.Number == startSection).First().Content;

				return frag;
			}
			else
			{			
				List<string> startTextList = searchData.SectionHeaders.Where(x => x.Number == startSection).Select(y => y.Title.ToLower()).ToList();

				List<string> stopTextList = searchData.SectionHeaders.Where(x => x.Number == stopSection).Select(y => y.Title.ToLower()).ToList();

				frag =  GetDocumentFragment(startTextList, stopTextList, textlines);

				if (!searchData.SectionList.Any(s => s.Number == startSection))
				{
					searchData.SectionList.Add(new Section(){Number = startSection, Content = frag});
				}

				return frag;
			}
		}

		private List<string> GetDocumentFragment(List<string> startTextList, List<string> stopTextList, List<string> textlines)
		{
			List<string> frag = new List<string>();
			int startline = 0;
			int stopline = 0;

			foreach (string startText in startTextList)
			{
				foreach (string line in textlines)
				{
					if (line.Contains(startText))
					{
						startline = textlines.IndexOf(line) + 1;
						break;
					}
				}
				if (startline > 0) break;
			}

			if (startline > 0)
			{
				foreach (string stopText in stopTextList)
				{
					foreach (string line in textlines.Skip(startline))
					{
						if (line.Contains(stopText))
						{
							stopline = textlines.IndexOf(line);
							break;
						}
					}
					if (stopline > 0) break;
				}

				if (stopline > 0)
				{
					frag = textlines.Skip(startline).Take(stopline - startline).ToList();
				}

			}
			return frag;
		}

		public bool IsSomeNumbers(string input)
		{
			if (string.IsNullOrEmpty(input)) return false;
			return input.Any(char.IsDigit);
		}

		public bool IsOnlyNumbers(string input)
		{
			if (string.IsNullOrEmpty(input)) return false;
			return input.All(char.IsDigit);
		}

/* 		public bool IsNumber(string input)
		{
			return Regex.IsMatch(input, @"\d");
		} */

		public List<string> GetCASNumbers(string input)
		{
			string casRegex = @"\b[1-9]{1}[0-9]{1,5}-\d{2}-\d\b";

			List<string> casnumbers = new List<string>();

			bool ret = Regex.IsMatch(input, casRegex);

			if (ret)
			{
				MatchCollection matchColl = Regex.Matches(input, casRegex);

				foreach (Match match in matchColl)
				{
					casnumbers.Add(match.Value);
				}
			}

			return casnumbers;
		}


		public ResultEntity Validate(ResultEntity result, string application, string requestGuid, string content, string keywordsJson)
		{
			if (string.IsNullOrEmpty(application))
			{
				result.ExceptionMessage = "Application argument cannot be empty";
			}

			if (Guid.Parse(requestGuid) == Guid.Empty)
			{
				result.ExceptionMessage = "RequestGuid argument cannot be empty or an empty guid";
			}

			if (string.IsNullOrEmpty(content))
			{
				result.ExceptionMessage = "Content argument cannot be empty";
			}

			if (string.IsNullOrEmpty(keywordsJson))
			{
				result.ExceptionMessage = "Keywords argument cannot be empty";
			}

			return result;
		}


        public int CalculateDocItemScore(ResultEntity entity)
		{
			int attrScore = entity.DocItems.Sum(x => x.Score);
			return Math.Min(attrScore,settings.MaxAttributeScore);
		}

		public int CalculateFormulaScore(ResultEntity entity)
		{			
			int formScore = entity.FormulaItems.Sum(x => x.Score);
			return Math.Min(formScore,settings.MaxAttributeScore);
		}
        
        public List<Component> GetKnownComponents()
		{
			Log.Debug("Getting known components from xml file");

			List<Component> comps = new List<Component>();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(@"Data/components.xml");
			XmlNode root = xdoc.DocumentElement;

			foreach (XmlNode node in root.SelectNodes("component"))
			{
				comps.Add(new Component()
				{
					CAS = node.SelectSingleNode("cas").InnerText,
					ChemName = node.SelectSingleNode("chemname").InnerText
				});
			}

			return comps;
		}


		public List<SectionHeader> GetSectionHeaders()
		{
			Log.Debug("Getting section headers from xml file");

			List<SectionHeader> headers = new List<SectionHeader>();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(@"Data/section_headers.xml");
			XmlNode root = xdoc.DocumentElement;

			foreach (XmlNode node in root.SelectNodes("section"))
			{
				foreach (XmlNode titleNode in node.SelectNodes("title"))
				{
					headers.Add(new SectionHeader()
					{
						Number = node.Attributes["id"].Value,
						Title = titleNode.InnerText.ToLower()
					});
				}
			}

			return headers;
		}

		public List<OtherIdentifier> GetOtherIdentifiers()
		{
			List<OtherIdentifier> oids = new List<OtherIdentifier>();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(@"Data/identifiers.xml");
			XmlNode root = xdoc.DocumentElement;

			foreach (XmlNode node in root.SelectNodes("category"))
			{
				foreach (XmlNode titleNode in node.SelectNodes("id"))
				{
					oids.Add(new OtherIdentifier()
					{
						Parent = node.Attributes["id"].Value,
						Id = titleNode.InnerText
					});
				}
			}

			return oids;
		}
        
        public SearchSet GetSearchSet(string xml)
		{
			SearchSet searchSet = new SearchSet();

			XmlSerializer serializer = new XmlSerializer(typeof(SearchSet), new XmlRootAttribute("SearchSet"));
			using (TextReader reader = new StringReader(xml))
			{
				searchSet = (SearchSet) serializer.Deserialize(reader);
			}

			return searchSet;
		}


		public void SaveResultToLog(ResultEntity result, string content, string requestGuid, 
										string origFileName, string application)
		{
			using (StreamWriter sw = File.AppendText($"{settings.FilesFolder}result_log.txt")) 
			{
				string output = $@"	{DateTime.Now} :: {application} :: {origFileName} :: {requestGuid} :: {result.FormulaItems.Count} :: {result.FormulaScore} :: {result.DocItems.Where(x =>x.Result.Length > 0).Count()} :: {result.DocItemScore} ";

				sw.WriteLine(output);
			}
		}

	 	public void SaveResultToAzure(ResultEntity result, string content, string requestGuid)
		{
			StorageEngine storageEngine = new StorageEngine(settings);

			int responseCode = storageEngine.AddResultToAzure(result, content);

			if (responseCode != 204)
			{
				result.ExceptionMessage = "Storing results to Azure failed";
				result.Messages.Add(result.ExceptionMessage);
				Log.Error(result.ExceptionMessage, $"Exception for request {requestGuid}");
			}
		}

    }

}