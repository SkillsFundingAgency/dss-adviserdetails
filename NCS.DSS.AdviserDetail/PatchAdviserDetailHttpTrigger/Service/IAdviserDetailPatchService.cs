using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public interface IAdviserDetailPatchService
    {
        string Patch(string adviserDetailJson, AdviserDetailPatch adviserDetailPatch);
    }
}
