using NCS.DSS.AdviserDetail.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public interface IPatchAdviserDetailHttpTriggerService
    {
        string PatchResource(string adviserDetailJson, AdviserDetailPatch adviserDetailPatch);
        Task<Models.AdviserDetail> UpdateCosmosAsync(string adviserDetail, Guid adviserDetailId);
        Task<string> GetAdviserDetailByIdAsync(Guid adviserDetailId);
    }
}