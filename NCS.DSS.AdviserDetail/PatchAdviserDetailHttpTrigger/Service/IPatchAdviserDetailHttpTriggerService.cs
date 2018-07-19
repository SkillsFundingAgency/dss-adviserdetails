using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public interface IPatchAdviserDetailHttpTriggerService
    {
        Task<Models.AdviserDetail> UpdateAsync(Models.AdviserDetail adviserDetail, Models.AdviserDetailPatch adviserDetailPatch);
        Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId);
    }
}