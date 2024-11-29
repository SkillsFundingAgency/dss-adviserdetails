using Microsoft.Azure.Cosmos;

namespace NCS.DSS.AdviserDetail.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId);
        Task<ItemResponse<Models.AdviserDetail>> CreateAdviserDetailAsync(Models.AdviserDetail adviserDetailId);
        Task<ItemResponse<Models.AdviserDetail>> UpdateAdviserDetailAsync(string adviserDetailJson, Guid adviserDetailId);
        Task<string> GetAdviserDetailsByIdToUpdateAsync(Guid adviserDetailId);
    }
}