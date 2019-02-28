using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public class AdviserDetailPatchService : IAdviserDetailPatchService
    {
        public string Patch(string adviserDetailJson, AdviserDetailPatch adviserDetailPatch)
        {
            if (string.IsNullOrEmpty(adviserDetailJson))
                return null;

            var obj = JObject.Parse(adviserDetailJson);

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserName))
                JsonHelper.UpdatePropertyValue(obj["AdviserName"], adviserDetailPatch.AdviserName);

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserEmailAddress))
                JsonHelper.UpdatePropertyValue(obj["AdviserEmailAddress"], adviserDetailPatch.AdviserEmailAddress);

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserContactNumber))
                JsonHelper.UpdatePropertyValue(obj["AdviserContactNumber"], adviserDetailPatch.AdviserContactNumber);

            if (adviserDetailPatch.LastModifiedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], adviserDetailPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(adviserDetailPatch.LastModifiedTouchpointId))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], adviserDetailPatch.LastModifiedTouchpointId);

            return obj.ToString();

        }
    }
}