using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.AdviserDetail.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.DeleteAdviserDetailHttpTrigger
{
    public static class DeleteAdviserDetailHttpTrigger
    {
        [Disable]
        [FunctionName("Delete")]
        [AdviserDetailResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Detail Deleted", ShowSchema = true)]
        [AdviserDetailResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Adviser Detail Id does not exist", ShowSchema = false)]
        [Display(Name = "Delete", Description = "Ability to delete an adviser details record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "AdviserDetails/{adviserDetailId}")]HttpRequestMessage req, TraceWriter log, string adviserDetailId)
        {
            log.Info("Delete Adviser Detail C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Deleted Adviser Detail record with Id of : " + adviserDetailGuid)
            };
        }
    }
}