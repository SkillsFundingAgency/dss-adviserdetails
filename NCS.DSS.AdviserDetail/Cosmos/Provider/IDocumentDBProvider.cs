using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId);
        Task<ResourceResponse<Document>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetailId);
        Task<ResourceResponse<Document>> UpdateAdviserDetailAsync(string adviserDetailJson, Guid adviserDetailId);
        Task<string> GetAdviserDetailsByIdToUpdateAsync(Guid adviserDetailId);
    }
}