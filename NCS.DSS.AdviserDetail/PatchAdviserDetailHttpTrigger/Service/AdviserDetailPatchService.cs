using DFC.JSON.Standard;
using NCS.DSS.AdviserDetail.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public class AdviserDetailPatchService : IAdviserDetailPatchService
    {
        private readonly IJsonHelper _jsonHelper;

        public AdviserDetailPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public string Patch(string adviserDetailJson, AdviserDetailPatch adviserDetailPatch)
        {
            if (string.IsNullOrEmpty(adviserDetailJson))
                return null;

            var obj = JObject.Parse(adviserDetailJson);

            if (!string.IsNullOrEmpty(adviserDetailPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", adviserDetailPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], adviserDetailPatch.SubcontractorId);
            }

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserName))
                _jsonHelper.UpdatePropertyValue(obj["AdviserName"], adviserDetailPatch.AdviserName);

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserEmailAddress))
                _jsonHelper.UpdatePropertyValue(obj["AdviserEmailAddress"], adviserDetailPatch.AdviserEmailAddress);

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserContactNumber))
                _jsonHelper.UpdatePropertyValue(obj["AdviserContactNumber"], adviserDetailPatch.AdviserContactNumber);

            if (adviserDetailPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], adviserDetailPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(adviserDetailPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], adviserDetailPatch.LastModifiedTouchpointId);

            return obj.ToString();

        }
    }
}