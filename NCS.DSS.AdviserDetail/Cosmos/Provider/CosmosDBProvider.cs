using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Models;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace NCS.DSS.AdviserDetail.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _container;
        private readonly string _databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private readonly string _containerId = Environment.GetEnvironmentVariable("CollectionId");
        private readonly ILogger<CosmosDBProvider> _logger;
        public CosmosDBProvider(CosmosClient cosmosClient, ILogger<CosmosDBProvider> logger)
        {
            _container = cosmosClient.GetContainer(_databaseId, _containerId);
            _logger = logger;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                var queryCust = _container.GetItemLinqQueryable<Models.Customer>().Where(x => x.CustomerId == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    if (response != null)
                        return true;
                }
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError("Failed to find the Customer Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }

        }

        public async Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            try
            {
                var queryCust = _container.GetItemLinqQueryable<Models.AdviserDetail>().Where(x => x.AdviserDetailId == adviserDetailId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    return response.Resource.FirstOrDefault();
                }
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError("Failed to find the Advisor Detail Record in Cosmos DB {AdvisorId}. Exception {Exception}", adviserDetailId, ce.Message);
                throw;
            }

        }

        public async Task<string> GetAdviserDetailsByIdToUpdateAsync(Guid adviserDetailId)
        {
            try
            {
                var queryCust = _container.GetItemLinqQueryable<Models.AdviserDetail>().Where(x => x.AdviserDetailId == adviserDetailId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    return JsonSerializer.Serialize(response.Resource.FirstOrDefault());
                }
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError("Failed to find the Customer Record for update in Cosmos DB {AdvisorId}. Exception {Exception}", adviserDetailId, ce.Message);
                throw;
            }           
        }


        public async Task<ItemResponse<Models.AdviserDetail>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetail)
        {
            var response = await _container.CreateItemAsync(adviserDetail, null);
            return response;
        }

        public async Task<ItemResponse<Models.AdviserDetail>> UpdateAdviserDetailAsync(string adviserDetailJson, Guid adviserDetailId)
        {
            var advisor = JsonSerializer.Deserialize<Models.AdviserDetail>(adviserDetailJson);
            return await _container.ReplaceItemAsync(advisor, adviserDetailId.ToString());
        }

    }
}