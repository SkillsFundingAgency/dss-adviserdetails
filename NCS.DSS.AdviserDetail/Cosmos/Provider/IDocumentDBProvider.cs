using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.AdviserDetail.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId);
        Task<ResourceResponse<Document>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetailId);
        Task<ResourceResponse<Document>> UpdateAdviserDetailAsync(Models.AdviserDetail adviserDetailId);
        Task<string> GetAdviserDetailsByIdToUpdateAsync(Guid adviserDetailId);
    }
}