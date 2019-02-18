using DFC.JSON.Standard;
using NCS.DSS.AdviserDetail.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service
{
    public class AdviserDetailPatchService : IAdviserDetailPatchService
    {
        private IJsonHelper _jsonHelper;

        public AdviserDetailPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public Models.AdviserDetail Patch(string adviserdetailJson, AdviserDetailPatch adviserdetailPatch)
        {
            if (string.IsNullOrEmpty(adviserdetailJson))
                return null;

            var obj = JObject.Parse(adviserdetailJson);

            if (!string.IsNullOrEmpty(adviserdetailPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", adviserdetailPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], adviserdetailPatch.SubcontractorId);
            }

            if (!string.IsNullOrEmpty(adviserdetailPatch.AdviserName))
                _jsonHelper.UpdatePropertyValue(obj["AdviserName"], adviserdetailPatch.AdviserName);

            if (!string.IsNullOrEmpty(adviserdetailPatch.AdviserEmailAddress))
                _jsonHelper.UpdatePropertyValue(obj["AdviserEmailAddress"], adviserdetailPatch.AdviserEmailAddress);

            if (!string.IsNullOrEmpty(adviserdetailPatch.AdviserContactNumber))
                _jsonHelper.UpdatePropertyValue(obj["AdviserContactNumber"], adviserdetailPatch.AdviserContactNumber);

            if (adviserdetailPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], adviserdetailPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(adviserdetailPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], adviserdetailPatch.LastModifiedTouchpointId);

            return obj.ToObject<Models.AdviserDetail>();

        }
    }
}
