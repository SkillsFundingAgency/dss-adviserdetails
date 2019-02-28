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

        public string PatchResource(string adviserDetailJson, AdviserDetailPatch adviserDetailPatch)
        {
            if (string.IsNullOrEmpty(adviserDetailJson))
                return null;

            if (adviserDetailPatch == null)
                return null;

            adviserDetailPatch.SetDefaultValues();

            var updatedAdviserDetail = _adviserdetailPatchService.Patch(adviserDetailJson, adviserDetailPatch);

            return updatedAdviserDetail;
        }

        public async Task<Models.AdviserDetail> UpdateCosmosAsync(string adviserDetail, Guid adviserDetailId)
        {
            if (adviserDetail == null)
                return null;

            var response = await _documentDbProvider.UpdateAdviserDetailAsync(adviserDetail, adviserDetailId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetAdviserDetailByIdAsync(Guid adviserDetailId)
        {
            return await _documentDbProvider.GetAdviserDetailsByIdToUpdateAsync(adviserDetailId);
        }

    }
}