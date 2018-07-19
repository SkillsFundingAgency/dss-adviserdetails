using System;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Cosmos.Provider;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service
{
    public class GetAdviserDetailByIdHttpTriggerService : IGetAdviserDetailByIdHttpTriggerService
    {
        public async Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var adviserDetail = await documentDbProvider.GetAdviserDetailByIdAsync(adviserDetailId);

            return adviserDetail;
        }
    }
}