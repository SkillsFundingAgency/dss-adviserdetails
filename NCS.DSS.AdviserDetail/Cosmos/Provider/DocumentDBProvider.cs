using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.AdviserDetail.Cosmos.Client;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.AdviserDetail.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public async Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(adviserDetailId);

            var client = DocumentDBClient.CreateDocumentClient();

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                if (response.Resource != null)
                    return (dynamic)response.Resource;
            }
            catch (DocumentClientException)
            {
                return null;
            }

            return null;

        }

        public async Task<string> GetAdviserDetailsByIdToUpdateAsync(Guid adviserDetailId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var adviserdetailForCustomerQuery = client
                ?.CreateDocumentQuery<Models.AdviserDetail>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.AdviserDetailId == adviserDetailId)
                .AsDocumentQuery();

            if (adviserdetailForCustomerQuery == null)
                return null;

            var adviserdetail = await adviserdetailForCustomerQuery.ExecuteNextAsync();

            return adviserdetail?.FirstOrDefault()?.ToString();
        }


        public async Task<ResourceResponse<Document>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetail)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, adviserDetail);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateAdviserDetailAsync(string adviserDetailJson, Guid adviserDetailId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(adviserDetailId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var adviserDetailDocumentJObject = JObject.Parse(adviserDetailJson);

            var response = await client.ReplaceDocumentAsync(documentUri, adviserDetailDocumentJObject);

            return response;
        }

    }
}