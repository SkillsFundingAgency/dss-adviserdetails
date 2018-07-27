using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public class PatchAdviserDetailHttpTriggerService : IPatchAdviserDetailHttpTriggerService
    {
        public async Task<Models.AdviserDetail> UpdateAsync(Models.AdviserDetail adviserDetail, AdviserDetailPatch adviserDetailPatch)
        {
            if (adviserDetail == null)
                return null;

            adviserDetailPatch.SetDefaultValues();
            adviserDetail.Patch(adviserDetailPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateAdviserDetailAsync(adviserDetail);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? adviserDetail : null;
        }

        public async Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var adviserDetail = await documentDbProvider.GetAdviserDetailByIdAsync(adviserDetailId);

            return adviserDetail;
        }
    }
}