using System.Linq;
using DataMinerAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using Serilog;

namespace DataMinerAPI.Engine
{
    public class DocItemBuilder
	{    
        private readonly IMemoryCache cache;
		private readonly ServiceSettings settings;

        private Helpers helpers;

        public DocItemBuilder(IMemoryCache _cache, ServiceSettings _settings)
        {
            cache = _cache;
            settings = _settings;

            helpers = new Helpers(cache, settings);
        }         

        public DocItem SearchForItem(DocItem docItem, List<string> textlines, SearchableContent searchData)
		{			
			foreach (string searchText in docItem.Terms)
			{
				int currentLine = 0;
			
				string termType = docItem.Hint.ToLower();
/* 
				if (helpers.IsOnlyNumbers(docItem.Section))
				{
					int stopSecton = int.Parse(docItem.Section);
					stopSecton = stopSecton++;
					textlines = helpers.GetSection(textlines, docItem.Section, stopSecton.ToString(), searchData);
				} */

				foreach (string line in textlines)
				{
					string evalLine = line.ToLower();					

					if (evalLine.Contains(searchText.ToLower()))
					{
						string restofLine = evalLine.Substring(evalLine.IndexOf(searchText) + searchText.Length + 1).Trim();

						if (termType == "number")
						{
							List<string> evalWords = evalLine.Split(" ").ToList();
							evalWords.RemoveAll(x => x.Trim() == string.Empty);

							docItem = CheckValue(docItem, searchText, restofLine,
												evalWords, line.ToLower(), textlines, currentLine);

						}
						else if (termType == "text")
						{
							docItem = CheckText(docItem, restofLine);
						}					
						else if (termType == "yesno")
						{
							docItem = CheckBool(docItem, restofLine);
						}
						else
						{
							docItem = CheckText(docItem, restofLine);
						}
					}

					if (!string.IsNullOrEmpty(docItem.Result))
					{
						Log.Debug($"[Found DocItem Term]: {searchText} [Score]: {docItem.Score} [Result]: {docItem.Result} ");
						return docItem;
					}
					else
					{
						Log.Debug($"[Missed DocItem Term]: {searchText} ");
					}

					currentLine++;
				}
			}

			return docItem;
		}

		private DocItem CheckValue(DocItem docItem, string searchText, string restofLine,
										List<string> evalWords, string line, List<string> textlines, int currentLine)
		{
			/* look for values as attributes of docItem Terms

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
			if (helpers.IsOnlyNumbers(restofLine))
			{
				docItem.Result = restofLine;
				docItem.Score = (int) Scores.HIGH_SCORE;
			}
			//	if there are somw numbers in the rest of the line then medium chance this is
			//	the value we are looking for since it could be mixed with uom, method et al
			else if (helpers.IsSomeNumbers(restofLine))
			{
				// get just the next word if it is numeric

				if (evalWords.Count > 1)
				{
					string nextWord = evalWords[evalWords.IndexOf(searchText) + searchText.Split(" ").Length + 1];
					//Search 2
					if (helpers.IsSomeNumbers(nextWord))
					{
						docItem.Result = nextWord ;
						docItem.Score = (int) Scores.MEDIUM_SCORE;
					}
					else
					{
						docItem.Result = string.Empty; ;
						docItem.Score = (int) Scores.NO_SCORE;
					}
				}
			}
			else	 //look below the found search term in case it is a tabular layout
			{
				int positionInLine = line.IndexOf(searchText);

				if (currentLine == textlines.Count)
				{
					 return docItem;
				}

				string nextLineText = textlines[currentLine + 1];

				//Search 3
				if ((helpers.IsSomeNumbers(nextLineText)) && (nextLineText.Length < 20))
				{
					docItem.Result = nextLineText;
					docItem.Score = (int) Scores.MEDIUM_SCORE;
				}
				//Search 4
				else  if (nextLineText.Length >= line.Length)
				{
					if (positionInLine != -1)
					{
						docItem.Result = nextLineText.Substring(positionInLine);
						docItem.Score = (int) Scores.LOW_SCORE;
					}
				}
				else
				{
					docItem.Result = string.Empty; ;
					docItem.Score = (int) Scores.NO_SCORE;
				}
			}

			return docItem;
		}


		private DocItem CheckText(DocItem docItem, string restofLine)
		{

			if (restofLine.Length >= 2)
			{
				docItem.Result = restofLine;
				docItem.Score = (int) Scores.MEDIUM_SCORE;
			}
			else if(restofLine.Length == 0)
			{
				docItem.Result = string.Empty;
				docItem.Score = (int) Scores.NO_SCORE;
			}

			return docItem;
		}

		private DocItem CheckBool(DocItem docItem, string restofLine)
		{

			if (restofLine == "yes" || restofLine == "true" )
			{
				docItem.Result = restofLine;
				docItem.Score = (int) Scores.MEDIUM_SCORE;
			}
			else if(restofLine.Length == 0)
			{
				docItem.Result = string.Empty;
				docItem.Score = (int) Scores.NO_SCORE;
			}

			return docItem;
		}
    }
}