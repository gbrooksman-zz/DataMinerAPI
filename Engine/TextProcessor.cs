using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using DataMinerAPI.Models;
using System.Text.Json;

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
		private SearchableContent searchData;
		private FormulaBuilder formulaBuilder;

		public TextProcessorEngine(IMemoryCache _cache, ServiceSettings _settings)
		{
			cache = _cache;
			settings = _settings;

			helpers = new Helpers(cache, settings);
			docItemBuilder = new DocItemBuilder(cache, settings);
			formulaBuilder = new FormulaBuilder(cache, settings);

			searchData = new SearchableContent(){	KnownChemicals = new List<Component>(),
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


		public SearchResults ProcessDocumentContent(string docContent, string keywordsJSON, string requestGuid, string application, string origFileName)
		{	
			// the object that will be returned by this engine 
			SearchResults returnEntity = new SearchResults()
			{
				DocItems = new List<DocItem>(),
				FormulaItems = new List<FormulaItem>(),
				Messages = new List<string>(),
				FileName = origFileName,
				RequestGuid = requestGuid,
				Application = application
			};

			try
			{	
				returnEntity = helpers.Validate(returnEntity, application, requestGuid, docContent, keywordsJSON);

				if (!returnEntity.Success)
				{
					Log.Error(returnEntity.AppException, "in Validation");
					return returnEntity;
				}

				//	contains all of the parameters necessary to determine what to look for in the provided document
				SearchCriteria searchSet = JsonSerializer.Deserialize<SearchCriteria>(keywordsJSON);

				List<string> lines = docContent.Split(Environment.NewLine).ToList();

				List<string> textlines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select( y => y.ToLower()).ToList();

				returnEntity.DocItems.AddRange(from DocItem searchTerm in searchSet.DocItems
												select docItemBuilder.SearchForItem(searchTerm, textlines, searchData));

				returnEntity.Messages.Add($"Count of detected DocItems: {returnEntity.DocItems.Where(s => !string.IsNullOrEmpty(s.Result)).Count()}");

				if (searchSet.DoFormula)
				{
					returnEntity.DoFormula = true;
					returnEntity.FormulaItems.AddRange(formulaBuilder.GetFormulation(textlines, requestGuid, searchData));
					returnEntity.Messages.Add($"Count of detected Formula Items: {returnEntity.FormulaItems.Count}");
				}				
				
				returnEntity.DocItemScore = helpers.CalculateDocItemScore(returnEntity);
				returnEntity.FormulaScore = helpers.CalculateFormulaScore(returnEntity);
				returnEntity.DateStamp = DateTime.Now;
				returnEntity.Success = true;

				/* if (settings.SaveToAzure)
				{
					returnEntity.Messages.Add($"Saved results to Azure");
					helpers.SaveResultToAzure(returnEntity, docContent, requestGuid);
				}	 */

				if (settings.SaveToLog)
				{
					returnEntity.Messages.Add($"Saved results to log file");
					helpers.SaveResultToLog(returnEntity, docContent, requestGuid, origFileName, application);
				}	
			}
			catch (Exception ex)
			{
				ProcessDocumentContentException procEx = new ProcessDocumentContentException("ProcessDocumentContent", ex);				
				Log.Error(procEx,ex.Message );				
				throw procEx;
			}

			return returnEntity;
		}
	}
}
