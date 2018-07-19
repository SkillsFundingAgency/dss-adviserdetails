using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.AdviserDetail.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}