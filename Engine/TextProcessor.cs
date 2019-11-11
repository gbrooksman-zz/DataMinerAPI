using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using DataMinerAPI.Models;

namespace DataMinerAPI.Engine
{
	/// <summary>
	///
	/// </summary>
	public class TextProcessorEngine
	{
		private readonly List<string> _lines = new List<string>();

		private List<Component> knownChemicals = new List<Component>();
		private List<SectionHeader> sectionHeaders = new List<SectionHeader>();
		private List<OtherIdentifier> otherIdentifiers = new List<OtherIdentifier>();

		private List<string> listofCASTerms = new List<string>();

		//private List<string> listofFormulaStartTerms = new List<string>();

		//private List<string> listofFormulaStopTerms = new List<string>();

		//private readonly List<string> listofTransportStartTerms = new List<string>();

		//private readonly List<string> listofTransportStopTerms = new List<string>();

		private IMemoryCache cache;
		private readonly IConfiguration config;
		private readonly AppOptions options;

		private const int HIGH_SCORE = 10;
		private const int MEDIUM_SCORE = 6;
		private const int LOW_SCORE = 3;
		private const int NO_SCORE = 0;

		/// <summary>
		///
		/// </summary>
		/// <param name="_cache"></param>
		/// <param name="_config"></param>
		/// <param name="_options"></param>
		public TextProcessorEngine(IMemoryCache _cache, IConfiguration _config, AppOptions _options)
		{
			cache = _cache;
			config = _config;
			options = _options;

			knownChemicals = cache.GetOrCreate<List<Component>>("knownComponents",
			   cacheEntry =>
			   {
				   return GetKnownComponents();
			   });

			sectionHeaders = cache.GetOrCreate<List<SectionHeader>>("sectionHeaders",
			   cacheEntry =>
			   {
				   return GetSectionHeaders();
			   });

			otherIdentifiers = cache.GetOrCreate<List<OtherIdentifier>>("otherIdentifiers",
			   cacheEntry =>
			   {
				   return GetOtherIdentifiers();
			   });

			//get the possible identifiers for cas numbers
			listofCASTerms.AddRange(otherIdentifiers.Where(o =>o.Parent == "cas").Select(s=>s.Id).ToList());

			//get the possible titles for section 3
			//listofFormulaStartTerms.AddRange(sectionHeaders.Where(x=> x.Number == "3").Select(s=>s.Title).ToList());

			//get the possible titles for section 4
			//listofFormulaStopTerms.AddRange(sectionHeaders.Where(x => x.Number == "4").Select(s => s.Title).ToList());

		}

		private List<Component> GetKnownComponents()
		{
			Log.Information("Getting known components from xml file");

			List<Component> comps = new List<Component>();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(@"data/components.xml");
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


		private List<SectionHeader> GetSectionHeaders()
		{
			Log.Information("Getting section headers from xml file");

			List<SectionHeader> headers = new List<SectionHeader>();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(@"data/section_headers.xml");
			XmlNode root = xdoc.DocumentElement;

			foreach (XmlNode node in root.SelectNodes("section"))
			{
				foreach (XmlNode titleNode in node.SelectNodes("title"))
				{
					headers.Add(new SectionHeader()
					{
						Number = node.Attributes["id"].Value,
						Title = titleNode.InnerText
					});
				}
			}

			return headers;
		}

		private List<OtherIdentifier> GetOtherIdentifiers()
		{
			List<OtherIdentifier> oids = new List<OtherIdentifier>();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(@"data/identifiers.xml");
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

		/// <summary>
		///
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>

		public int CalculateScore(ResultEntity entity)
		{
			int attrScore = entity.SearchTerms.Sum(x => x.Score) + entity.FormulaItems.Sum(x => x.Score);

			if (options.MaxAttributeScore > 0)
			{
				attrScore = attrScore > options.MaxAttributeScore ? options.MaxAttributeScore : attrScore;
			}

			int formScore = entity.FormulaItems.Sum(x => x.Score) + entity.FormulaItems.Sum(x => x.Score);

			if (options.MaxFormulaScore > 0)
			{
				formScore = formScore > options.MaxFormulaScore ? options.MaxFormulaScore : formScore;
			}

			return (attrScore + formScore);
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="content"></param>
		/// <param name="keywordsJson"></param>
		/// <param name="requestGuid"></param>
		/// <param name="application"></param>
		/// <returns></returns>
		public ResultEntity ProcessContent(string content, string keywordsJson, string requestGuid, string application)
		{
			ResultEntity result = new ResultEntity(requestGuid, application)
			{
				SearchTerms = new List<SearchTerm>(),
				FormulaItems = new List<FormulaItem>(),
				Messages = new List<string>()
			};

			result = Validate(result, application, requestGuid, content, keywordsJson);

			if (result.Exception != null)
			{
				result.Messages.Add(result.Exception.Message);
				Log.Error(result.Exception, $"Exception for request {requestGuid}");
				return result;
			}

			List<SearchTerm> searchResults = new List<SearchTerm>();

			//-------------------------------------------------------------------------------------------------------------

			List<string> lines = content.Split(Environment.NewLine).ToList();

			List<string> textlines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select( y=>y.ToLower()).ToList();

			List<SearchTerm> searchTerms = JsonConvert.DeserializeObject<List<SearchTerm>>(keywordsJson);

			foreach (SearchTerm searchTerm in searchTerms)
			{
				result.SearchTerms.Add(FindSearchTerm(searchTerm, textlines));
			}

			result.Messages.Add($"Count of Keywords: {result.SearchTerms.Where(s => !string.IsNullOrEmpty(s.Value)).Count()}");

			result.FormulaItems.AddRange(GetFormulation(textlines, requestGuid));

			result.Messages.Add($"Count of Formula Items: {result.FormulaItems.Count}");

			result.Score = CalculateScore(result);
			result.DateStamp = DateTime.Now;

			if (options.SaveToAzure)
			{
				result.Messages.Add($"Saved results to Azure");
				SaveResult(result, content, requestGuid);
			}

			if (options.SaveToLocalSQL)
			{
				result.Messages.Add($"Saved results to LocalSQL");
				SaveResultSQL(result, content, requestGuid);
			}

			return result;
		}


		private void SaveResult(ResultEntity result, string content, string requestGuid)
		{
			StorageEngine storageEngine = new StorageEngine(config);

			int responseCode = storageEngine.AddResultToAzure(result, content);

			if (responseCode != 204)
			{
				result.Exception = new InvalidOperationException("Storing results failed");
				result.Messages.Add(result.Exception.Message);
				Log.Error(result.Exception, $"Exception for request {requestGuid}");
			}
		}

		private void SaveResultSQL(ResultEntity result, string content, string requestGuid)
		{			
            StorageEngine storageEngine = new StorageEngine(config);

			int responseCode = storageEngine.AddResultToAzure(result, content);

			if (responseCode != 204)
			{
				result.Exception = new InvalidOperationException("Storing results failed");
				result.Messages.Add(result.Exception.Message);
				Log.Error(result.Exception, $"Exception for request {requestGuid}");
			}
		}

		private ResultEntity Validate(ResultEntity result, string application, string requestGuid, string content, string keywordsJson)
		{
			if (string.IsNullOrEmpty(application))
			{
				result.Exception = new ArgumentNullException("Application argument cannot be empty");
			}

			if (Guid.Parse(requestGuid) == Guid.Empty)
			{
				result.Exception = new ArgumentOutOfRangeException("RequestGuid argument cannot be empty or an empty guid");
			}

			if (string.IsNullOrEmpty(content))
			{
				result.Exception = new ArgumentNullException("Content argument cannot be empty");
			}

			if (string.IsNullOrEmpty(keywordsJson))
			{
				result.Exception = new ArgumentNullException("Keywords argument cannot be empty");
			}

			return result;

		}

		private List<FormulaItem> GetFormulation (List<string> textlines ,string requestGuid)
		{
			List<FormulaItem> items = new List<FormulaItem>();

			List<string> formulaLines = GetSection(textlines, "3", "4");

			if (formulaLines.Count > 0)
			{
				items = BuildFormula(textlines);
			}
			else
			{

				if (items.Count == 0)
				{
					formulaLines = GetSection(textlines, "2", "3");		//try pre GHS structure
					if (formulaLines.Count > 0)
					{
						items = BuildFormula(textlines);
					}
					if (items.Count == 0)
					{
						Log.Information($"No formula section found for request {requestGuid}");
					}
				}
			}

			return items;

		}


		/// <summary>
		/// Get a document section or contiguous sections
		/// </summary>
		/// <param name="textlines"></param>
		/// <param name="startSection"></param>
		/// <param name="stopSection"></param>
		/// <returns></returns>

		public List<string> GetSection(List<string> textlines, string startSection, string stopSection)
		{
			List<SectionHeader> sectionHeaders = cache.GetOrCreate<List<SectionHeader>>("sectionHeaders",
			   cacheEntry =>
			   {
				   return GetSectionHeaders();
			   });

			List<string> startTextList = sectionHeaders.Where(x => x.Number == startSection).Select(y => y.Title.ToLower()).ToList();

			List<string> stopTextList = sectionHeaders.Where(x => x.Number == stopSection).Select(y => y.Title.ToLower()).ToList();

			return GetDocumentFragment(startTextList, stopTextList, textlines);

		}




		/// <summary>
		///
		/// </summary>
		/// <param name="startTextList"></param>
		/// <param name="stopTextList"></param>
		/// <param name="textlines"></param>
		/// <returns></returns>
		public List<string> GetDocumentFragment(List<string> startTextList, List<string> stopTextList, List<string> textlines)
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

		private List<FormulaItem> BuildFormula(List<string> lines)
		{
			List<FormulaItem> formulaItems = new List<FormulaItem>();

			foreach (string line in lines)
			{
				List<string> casArray = GetCASNumbers(line);

				foreach (string cas in casArray)
				{
					FormulaItem formulaItem = new FormulaItem
					{
						CASNumber = cas,
						OtherInfo = line
					};

					if (knownChemicals.Exists(c => c.CAS == cas))
					{
						formulaItem.ChemName = knownChemicals.Where(c => c.CAS == cas).FirstOrDefault().ChemName;
						formulaItem.Score = HIGH_SCORE;
					}
					else
					{
						formulaItem.ChemName = "NAME NOT FOUND";
						formulaItem.Score = MEDIUM_SCORE;
					}

					formulaItems.Add(formulaItem);
				}
			}

			return formulaItems;
		}

		/// <summary>
		/// returns the value of the first occurrence of the search term
		/// </summary>
		/// <param name="searchTerm"></param>
		/// <param name="textlines"></param>
		/// <returns></returns>
		private SearchTerm FindSearchTerm(SearchTerm searchTerm, List<string> textlines)
		{
			int currentLine = 0;
			string searchText = searchTerm.Item.ToLower();
			string termType = searchTerm.Type.ToLower();

			foreach (string line in textlines)
			{
				string evalLine = line.ToLower();
				List<string> evalWords = evalLine.Split(" ").ToList();
				evalWords.RemoveAll(x => x.Trim() == string.Empty);

				if (evalLine.Contains(searchText))
				{
					string restofLine = evalLine.Substring(evalLine.IndexOf(searchText) + searchText.Length + 1).Trim();

					if (termType == "number")
					{
						searchTerm = CheckValue(searchTerm, searchText, restofLine,
												evalWords, line.ToLower(), textlines, currentLine);

					}
					else if (termType == "text")
					{
						searchTerm = CheckText(searchTerm, restofLine);

					}
					else
					{
						searchTerm = CheckText(searchTerm, restofLine);

					}
				}

				if (!string.IsNullOrEmpty(searchTerm.Value))
				{
					return searchTerm;
				}

				currentLine++;
			}

				return searchTerm;
		}

		private SearchTerm CheckValue(SearchTerm searchTerm, string searchText, string restofLine,
										List<string> evalWords, string line, List<string> textlines, int currentLine)
		{
			/* look for values as attributes of searchTerms

				Search 1:

				|---------------|---------------------------|
				|  Search Term	| Line    Value				|
				|---------------|---------------------------|

				Search 2:

				|---------------|---------------|
				|  Search Term	| Word(Value)	|
				|---------------|---------------|


				Search 3:

				|---------------|
				|  Search Term	|
				|---------------|
				|-------------------|
				| Length (Value)	|
				|-------------------|

				Search 4:

				|---------------|
				|  Search Term	|
				|---------------|
				|-------------------------------------------|
				| Line Pos (Value)							|
				|-------------------------------------------|

			 */

			//	if there are only numbers in the rest of the line then high chance this is
			//	the value we are looking for
			//Search 1
			if (IsOnlyNumbers(restofLine))
			{
				searchTerm.Value = restofLine;
				searchTerm.Score = HIGH_SCORE;
			}
			//	if there are somw numbers in the rest of the line then medium chance this is
			//	the value we are looking for since it could be mixed with uom, method et al
			else if (IsSomeNumbers(restofLine))
			{
				// get just the next word if it is numeric
				string nextWord = evalWords[evalWords.IndexOf(searchText) + searchText.Split(" ").Length + 1];
				//Search 2
				if (IsSomeNumbers(nextWord))
				{
					searchTerm.Value = nextWord ;
					searchTerm.Score = MEDIUM_SCORE;
				}
				else
				{
					searchTerm.Value = string.Empty; ;
					searchTerm.Score = NO_SCORE;
				}
			}
			else	 //look below the found search term in case it is a tabular layout
			{
				int positionInLine = line.IndexOf(searchText);

				if (currentLine == textlines.Count)	 return searchTerm;

				string nextLineText = textlines[currentLine + 1];

				//Search 3
				if ((IsSomeNumbers(nextLineText)) && (nextLineText.Length < 20))
				{
					searchTerm.Value = nextLineText;
					searchTerm.Score = MEDIUM_SCORE;
				}
				//Search 4
				else  if (nextLineText.Length >= line.Length)
				{
					searchTerm.Value = nextLineText.Substring(positionInLine);
					searchTerm.Score = LOW_SCORE;
				}
				else
				{
					searchTerm.Value = string.Empty; ;
					searchTerm.Score = NO_SCORE;
				}
			}

			return searchTerm;
		}


		private SearchTerm CheckText(SearchTerm searchTerm, string restofLine)
		{

			if (restofLine.Length >= 2)
			{
				searchTerm.Value = restofLine;
				searchTerm.Score = MEDIUM_SCORE;
			}
			else if(restofLine.Length == 0)
			{
				searchTerm.Value = string.Empty;
				searchTerm.Score = NO_SCORE;
			}

			return searchTerm;
		}

		//private SearchTerm SearchTermEval(SearchTerm searchTerm, List<string> lines, List<string> words)
		//{
		//	int currentLine = 0;

		//	foreach (string line in lines)
		//	{
		//		string evalLine = line.ToLower();

		//		currentLine++;

		//		if (evalLine.Contains(searchTerm.Item.ToLower()))
		//		{
		//			string restofLine = evalLine.Substring(evalLine.IndexOf(searchTerm.Item.ToLower()) + searchTerm.Item.Length + 1).Trim();

		//			if (searchTerm.Type.ToLower() == "number")
		//			{
		//				if (IsNumber(restofLine))
		//				{
		//					searchTerm.Value = restofLine;
		//					searchTerm.Score = 10;
		//				}
		//				else
		//				{
		//					searchTerm.Value = restofLine;
		//					searchTerm.Score = 5;
		//				}
		//			}
		//			else if (searchTerm.Type.ToLower() == "text")
		//			{
		//				if (restofLine.Length > 3)
		//				{
		//					searchTerm.Value = restofLine;
		//					searchTerm.Score = 5;
		//				}
		//				else
		//				{
		//					searchTerm.Value = restofLine;
		//					searchTerm.Score = 3;
		//				}
		//			}
		//			else
		//			{

		//			}

		//			// found the term but its value has not yet been found
		//			if (string.IsNullOrEmpty(searchTerm.Value.Trim()))
		//			{
		//				string nextline = lines[currentLine];
		//				searchTerm.Value = nextline;
		//				searchTerm.Score = 5;
		//			}

		//			//	if we dont have a hit yet lets go through the words array and
		//			//	look for our term and then grab the value from the next line
		//			//	to see if it is right
		//			if (string.IsNullOrEmpty(searchTerm.Value.Trim()))
		//			{
		//				string searchingFor = searchTerm.Item.ToLower();

		//				int wc = 0;

		//				foreach (string word in words)
		//				{
		//					wc++;
		//					if (word == searchingFor)
		//					{
		//						for (int i = 0; i < 10; i++)
		//						{
		//							if (words[wc + i] != Environment.NewLine)
		//							{
		//								searchTerm.Value += $"{words[wc + i]} ";

		//								//this might be the end of the earch term's value
		//								if (searchTerm.Value.Contains(".") || searchTerm.Value.Contains(":"))
		//								{
		//									break;
		//								}
		//							}
		//						}
		//						searchTerm.Value = searchTerm.Value.Trim();
		//						searchTerm.Score = 3;
		//					}

		//				}

		//			}
		//		}

		//	}

		//	return searchTerm;
		//}


		private bool IsSomeNumbers(string input)
		{
			if (string.IsNullOrEmpty(input)) return false;
			return input.Any(char.IsDigit);
		}

		private bool IsOnlyNumbers(string input)
		{
			if (string.IsNullOrEmpty(input)) return false;
			return input.All(char.IsDigit);
		}



		private bool IsNumber(string input)
		{
			return Regex.IsMatch(input, @"\d");
		}



		private List<string> GetCASNumbers(string input)
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
	}
}