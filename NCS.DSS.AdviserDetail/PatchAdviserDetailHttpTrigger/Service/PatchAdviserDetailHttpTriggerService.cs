using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public class PatchAdviserDetailHttpTriggerService : IPatchAdviserDetailHttpTriggerService
    {
        private readonly IAdviserDetailPatchService _adviserdetailPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchAdviserDetailHttpTriggerService(IDocumentDBProvider documentDbProvider, IAdviserDetailPatchService adviserdetailPatchService)
        {
            _documentDbProvider = documentDbProvider;
            _adviserdetailPatchService = adviserdetailPatchService;
        }

        public Models.AdviserDetail PatchResource(string adviserdetailJson, AdviserDetailPatch AdviserDetailPatch)
        {
            if (string.IsNullOrEmpty(adviserdetailJson))
                return null;

            if (AdviserDetailPatch == null)
                return null;

            AdviserDetailPatch.SetDefaultValues();

            var updatedAdviserDetail = _adviserdetailPatchService.Patch(adviserdetailJson, AdviserDetailPatch);

            return updatedAdviserDetail;
        }

        public async Task<Models.AdviserDetail> UpdateCosmosAsync(Models.AdviserDetail adviserdetail)
        {
            if (adviserdetail == null)
                return null;

            var response = await _documentDbProvider.UpdateAdviserDetailAsync(adviserdetail);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            var AdviserDetail = await _documentDbProvider.GetAdviserDetailsByIdToUpdateAsync(adviserDetailId);

            return AdviserDetail;
        }

    }
}