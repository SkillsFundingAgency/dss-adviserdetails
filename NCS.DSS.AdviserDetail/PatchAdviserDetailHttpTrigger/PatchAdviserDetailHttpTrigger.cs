using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.AdviserDetail.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger
{
    public static class PatchAdviserDetailHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [AdviserDetailResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail Updated", ShowSchema = true)]
        [AdviserDetailResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Adviser Detail Id does not exist", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an adviser details record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "AdviserDetails/{adviserDetailId}")]HttpRequestMessage req, TraceWriter log, string adviserDetailId)
        {
            log.Info("Patch Adviser Detail C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(adviserDetailId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Updated Adviser Detail record with Id of : " + adviserDetailGuid)
            };
        }
    }
}