using System;
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
        public async Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(adviserDetailId);

            var client = DocumentDBClient.CreateDocumentClient();

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                if (response.Resource != null)
                    return (dynamic) response.Resource;
            }
            catch (DocumentClientException)
            {
                return null;
            }

            return null;

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

        public async Task<ResourceResponse<Document>> UpdateAdviserDetailAsync(Models.AdviserDetail adviserDetail)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(adviserDetail.AdviserDetailId.GetValueOrDefault());

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, adviserDetail);

            return response;
        }

    }
}