using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public interface IAdviserDetailPatchService
    {
        string Patch(string adviserDetailJson, AdviserDetailPatch adviserDetailPatch);
    }
}
