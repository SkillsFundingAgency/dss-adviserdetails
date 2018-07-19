using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.AdviserDetail.Cosmos.Client;
using NCS.DSS.AdviserDetail.Cosmos.Helper;

namespace NCS.DSS.AdviserDetail.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var adviserDetailQuery = client
                ?.CreateDocumentQuery<Models.AdviserDetail>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.AdviserDetailId == adviserDetailId)
                .AsDocumentQuery();

            if (adviserDetailQuery == null)
                return null;

            var adviserDetail = await adviserDetailQuery.ExecuteNextAsync<Models.AdviserDetail>();

            return adviserDetail?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetail)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, adviserDetail);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateAdviserDetailAsync(Models.AdviserDetail adviserDetail)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(adviserDetail.AdviserDetailId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, adviserDetail);

            return response;
        }

    }
}