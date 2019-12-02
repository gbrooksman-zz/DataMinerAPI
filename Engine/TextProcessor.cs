using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using DataMinerAPI.Models;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace DataMinerAPI.Engine
{
	/// <summary>
	///
	/// </summary>
	public class TextProcessorEngine
	{
		private readonly IMemoryCache cache;
		private readonly ServiceSettings settings;

		private Helpers helpers;
		private DocItemBuilder docItemBuilder;
		private SearchData searchData;
		private FormulaBuilder formulaBuilder;

		public TextProcessorEngine(IMemoryCache _cache, ServiceSettings _settings)
		{
			cache = _cache;
			settings = _settings;

			helpers = new Helpers(cache, settings);
			docItemBuilder = new DocItemBuilder(cache, settings);
			formulaBuilder = new FormulaBuilder(cache, settings);

			searchData = new SearchData(){	KnownChemicals = new List<Component>(),
											OtherIdentifiers = new List<OtherIdentifier>(),
											SectionHeaders = new List<SectionHeader>(),
											SectionList = new List<Section>() };


			searchData.KnownChemicals = cache.GetOrCreate<List<Component>>("knownComponents",
			   cacheEntry =>
			   {
				   return helpers.GetKnownComponents();
			   });

			searchData.SectionHeaders = cache.GetOrCreate<List<SectionHeader>>("sectionHeaders",
			   cacheEntry =>
			   {
				   return helpers.GetSectionHeaders();
			   });

			searchData.OtherIdentifiers = cache.GetOrCreate<List<OtherIdentifier>>("otherIdentifiers",			   	
				cacheEntry =>
				{
					return helpers.GetOtherIdentifiers();
				});
		}


		public ResultEntity ProcessDocumentContent(string docContent, string keywordsXML, string requestGuid, string application, string origFileName)
		{	
			// the object that will be returned by this engine 
			ResultEntity parsedElements = new ResultEntity(requestGuid, application)
			{
				DocItems = new List<DocItem>(),
				FormulaItems = new List<FormulaItem>(),
				Messages = new List<string>()
			};

			try
			{	
				parsedElements = helpers.Validate(parsedElements, application, requestGuid, docContent, keywordsXML);

				if (!parsedElements.Success)
				{
					Log.Error(parsedElements.AppException, "in Validation");
					return parsedElements;
				}

				//	contains all of the parameters necessary to determine what to look for in the provided document
				SearchSet searchSet = helpers.GetSearchSet(keywordsXML);

				List<string> lines = docContent.Split(Environment.NewLine).ToList();

				List<string> textlines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select( y => y.ToLower()).ToList();

				parsedElements.DocItems.AddRange(from DocItem searchTerm in searchSet.DocItems
												select docItemBuilder.SearchForItem(searchTerm, textlines));

				parsedElements.Messages.Add($"Count of detected DocItems: {parsedElements.DocItems.Where(s => !string.IsNullOrEmpty(s.Result)).Count()}");

				if (searchSet.DoFormula)
				{
					parsedElements.FormulaItems.AddRange(formulaBuilder.GetFormulation(textlines, requestGuid, searchData));
					parsedElements.Messages.Add($"Count of detected Formula Items: {parsedElements.FormulaItems.Count}");
				}

				parsedElements.DocItemScore = helpers.CalculateDocItemScore(parsedElements);
				parsedElements.FormulaScore = helpers.CalculateFormulaScore(parsedElements);
				parsedElements.DateStamp = DateTime.Now;
				parsedElements.Success = true;

				if (settings.SaveToAzure)
				{
					parsedElements.Messages.Add($"Saved results to Azure");
					helpers.SaveResultToAzure(parsedElements, docContent, requestGuid);
				}	

				if (settings.SaveToLog)
				{
					parsedElements.Messages.Add($"Saved results to log file");
					helpers.SaveResultToLog(parsedElements, docContent, requestGuid, origFileName, application);
				}	
			}
			catch (Exception ex)
			{
				Log.Error(ex, "In ProcessDocumentContent");				
				parsedElements.AppException = ex;
				parsedElements.Success = false;
				parsedElements.DocItemScore = 0;
				parsedElements.FormulaScore = 0;
				throw;
			}

			return parsedElements;
		}
	}
}
