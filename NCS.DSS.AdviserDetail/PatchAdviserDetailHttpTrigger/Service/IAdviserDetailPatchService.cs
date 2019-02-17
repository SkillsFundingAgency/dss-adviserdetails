using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public interface IAdviserDetailPatchService
    {
        Models.AdviserDetail Patch(string adviserdetailJson, AdviserDetailPatch adviserdetailPatch);
    }
}
