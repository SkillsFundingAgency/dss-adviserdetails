using System;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public interface IPatchAdviserDetailHttpTriggerService
    {
        Models.AdviserDetail PatchResource(string adviserdetailJson, AdviserDetailPatch AdviserDetailPatchPatch);
        Task<Models.AdviserDetail> UpdateCosmosAsync(Models.AdviserDetail adviserDetail);
        Task<string> GetAdviserDetailByIdAsync(Guid adviserDetailId);
    }
}