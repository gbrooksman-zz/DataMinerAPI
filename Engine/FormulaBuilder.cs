using System.Linq;
using Serilog;
using DataMinerAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace DataMinerAPI.Engine
{
    public class FormulaBuilder
	{   
        private readonly IMemoryCache cache;
		private readonly ServiceSettings settings;

        private Helpers helpers;

        public FormulaBuilder(IMemoryCache _cache, ServiceSettings _settings)
        {
            cache = _cache;
            settings = _settings;

            helpers = new Helpers(cache, settings);
        } 


        public List<FormulaItem> GetFormulation (List<string> textlines ,string requestGuid, SearchableContent searchData)
		{
			List<FormulaItem> items = new List<FormulaItem>();

			List<string> formulaLines = helpers.GetSection(textlines, "3", "4", searchData);

			if (formulaLines.Count > 0)
			{
				items = BuildFormula(formulaLines, searchData);
			}
			else
			{
				if (items.Count == 0)
				{
					formulaLines = helpers.GetSection(textlines, "2", "3", searchData);		//try pre GHS structure
					if (formulaLines.Count > 0)
					{
						items = BuildFormula(formulaLines, searchData);
					}
					if (items.Count == 0)
					{
						Log.Information($"No formula section found for request {requestGuid}");
					}
				}
			}

			return items;
		}	

		private List<FormulaItem> BuildFormula(List<string> lines, SearchableContent searchData)
		{
			List<FormulaItem> formulaItems = new List<FormulaItem>();

			foreach (string line in lines)
			{
				List<string> casArray = helpers.GetCASNumbers(line);

				foreach (string cas in casArray)
				{
					FormulaItem formulaItem = new FormulaItem
					{
						CASNumber = cas,
						OtherInfo = line
					};

					if (searchData.KnownChemicals.Exists(c => c.CAS == cas))
					{
						formulaItem.ChemName = searchData.KnownChemicals.Where(c => c.CAS == cas).FirstOrDefault().ChemName;
						formulaItem.Score = (int) Scores.HIGH_SCORE;
					}
					else
					{
						formulaItem.ChemName = "NAME NOT FOUND";
						formulaItem.Score = (int) Scores.MEDIUM_SCORE;
					}

					formulaItems.Add(formulaItem);
				}
			}

			return formulaItems;
		}
    }
}
