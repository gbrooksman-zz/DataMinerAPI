using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataMinerAPI.Models;

namespace DataMinerAPI.Engine
{
	/// <summary>
	///
	/// </summary>
	public class StorageEngine
	{
		private readonly CloudTable table;
		private readonly ServiceSettings settings;

		public StorageEngine( ServiceSettings _settings)
		{
			settings = _settings;

			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(settings.AzureStorageConnectionString);

			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

			table = tableClient.GetTableReference("ParsedDocuments");
		}


		#region azure methods

		/// <summary>
		///
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public int AddResultToAzure(ResultEntity entity, string content)
		{
			entity.Application = entity.Application.ToUpper();
			entity.Content = content;

			TableOperation insertOperation = TableOperation.Insert(entity);

			TableResult result = Task.Run(() => table.ExecuteAsync(insertOperation)).Result;

			return result.HttpStatusCode;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="requestGuid"></param>
		/// <param name="application"></param>
		/// <returns></returns>
		public ResultEntity GetEntityFromAzure(string requestGuid, string application)
		{
			TableOperation retrieveOperation = TableOperation.Retrieve<ResultEntity>(requestGuid, application);

			TableResult retrievedResult = Task.Run(() => table.ExecuteAsync(retrieveOperation)).Result;

			return (ResultEntity)retrievedResult.Result;

		}


		/// <summary>
		/// Gets a count of re
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		public int GetCountFromAzure(string application)
		{
			int count = 0;

			TableQuery<ResultEntity> query = new TableQuery<ResultEntity>()
		   .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, application))
		   .Select(new string[] { "PartitionKey" });

			// Print the fields for each customer.
			TableContinuationToken token = null;
			do
			{
				TableQuerySegment<ResultEntity> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;

				token = resultSegment.ContinuationToken;

				count += resultSegment.Results.Count();

			} while (token != null);

			return count;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		public List<ResultEntity> GetEntityListFromAzure(string application)
		{

			List<ResultEntity> entities = new List<ResultEntity>();

			TableQuery<ResultEntity> query = new TableQuery<ResultEntity>()
				.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, application));

			// Print the fields for each customer.
			TableContinuationToken token = null;
			do
			{
				TableQuerySegment<ResultEntity> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;

				token = resultSegment.ContinuationToken;

				foreach (ResultEntity entity in resultSegment.Results)
				{
					entities.Add(new ResultEntity()
					{
						Application = application,
						DateStamp = entity.Timestamp,
						FormulaItems = entity.FormulaItems,
						Messages = entity.Messages,
						RequestGuid = entity.RequestGuid,
						FormulaScore = entity.FormulaScore,
						DocItemScore = entity.DocItemScore,
						DocItems = entity.DocItems
					});

				}
			} while (token != null);

			return entities;
		}

		#endregion


		#region local sql server methods - no longer used

		/* public int AddResultToLocalSQL(ResultEntity entity, string content)
		{
			int ret = 0;
			entity.Application = entity.Application.ToUpper();
			entity.Content = content;

			string cmdText = @"INSERT INTO DOCUMENTRESULTS								
									RequestGuid,
									Application,
									Content,
									SearchTerms,
									FormulaItems,
									Score,
									Messages,
									DateStamp,
									Exception
								VALUES (?,?,?,?,?,?,?,?,?) ;";

			using (SqlCommand cmd = new SqlCommand(cmdText, conn))
			{

				cmd.Parameters.Add(entity.RequestGuid);
				cmd.Parameters.Add(entity.Application);
				cmd.Parameters.Add(entity.Content);
				cmd.Parameters.Add(entity.DocItems.ToString());
				cmd.Parameters.Add(entity.FormulaItems.ToString());
				cmd.Parameters.Add(entity.Score);
				cmd.Parameters.Add(entity.Messages.ToString());
				cmd.Parameters.Add(DateTime.Now);
				cmd.Parameters.Add(entity.Exception.ToString());

				conn.Open();
				ret = cmd.ExecuteNonQuery();
				conn.Close();
			}

			return ret;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="requestGuid"></param>
		/// <param name="application"></param>
		/// <returns></returns>
		public ResultEntity GetEntityFromLocalSQL(string requestGuid, string application)
		{
			ResultEntity retEntity = new ResultEntity();

			return retEntity;

		}
 */
		#endregion

	}
}
