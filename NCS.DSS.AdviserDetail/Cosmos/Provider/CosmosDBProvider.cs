using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using System.Net;
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
                    {
                        _logger.LogInformation("Customer Record found in Cosmos DB for {CustomerID}", customerId);
                        return true;
                    }
                }
                _logger.LogError("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"Failed to find the Customer Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
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
                    if (response != null)
                    {
                        _logger.LogInformation("Advisor Detail Record found in Cosmos DB for {AdviserDetailId}", adviserDetailId);
                        return response.Resource.FirstOrDefault();
                    }                    
                }
                _logger.LogError("No Advisor Detail found with {AdviserDetailId} in Cosmos DB", adviserDetailId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"Failed to find the Advisor Detail Record in Cosmos DB {AdvisorId}. Exception {Exception}", adviserDetailId, ce.Message);
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
                    if (response != null)
                    {
                        var jsonString = JsonSerializer.Serialize(response.Resource.FirstOrDefault());
                        _logger.LogInformation("Advisor Detail found in Cosmos DB for {AdviserDetailId}", adviserDetailId);
                        return jsonString;
                    }
                }
                _logger.LogError("No Advisor Detail found with {AdviserDetailId} in Cosmos DB", adviserDetailId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"Failed to find the Advisor Detail for update in Cosmos DB {AdvisorId}. Exception {Exception}", adviserDetailId, ce.Message);
                throw;
            }           
        }


        public async Task<ItemResponse<Models.AdviserDetail>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetail)
        {
            try
            {
                var response = await _container.CreateItemAsync(adviserDetail, null);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Advisor Detail Record Created in Cosmos DB for {AdviserDetailId}", adviserDetail.AdviserDetailId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Create Advisor Detail Record in Cosmos DB for {AdviserDetailId}", response.StatusCode, adviserDetail.AdviserDetailId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"Failed to Create Advisor Detail Record in Cosmos DB {AdviserDetailId}. Exception {Exception}.", adviserDetail.AdviserDetailId, ce.Message);
                throw;
            }            
        }

        public async Task<ItemResponse<Models.AdviserDetail>> UpdateAdviserDetailAsync(string adviserDetailJson, Guid adviserDetailId)
        {           
            try
            {
                var advisor = JsonSerializer.Deserialize<Models.AdviserDetail>(adviserDetailJson);
                var response = await _container.ReplaceItemAsync(advisor, adviserDetailId.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Advisor Detail Record Updated in Cosmos DB for {AdviserDetailId}", advisor.AdviserDetailId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Update Advisor Detail Record in Cosmos DB for {AdviserDetailId}", response.StatusCode, advisor.AdviserDetailId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce,"Failed to Update Advisor Detail Record in Cosmos DB {AdviserDetailId}. Exception {Exception}.", adviserDetailId, ce.Message);
                throw;
            }
        }

    }
}