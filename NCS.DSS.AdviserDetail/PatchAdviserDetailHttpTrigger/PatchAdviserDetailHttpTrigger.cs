using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger
{
    public static class PatchAdviserDetailHttpTrigger
    {
        [FunctionName("Patch")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "AdviserDetails/{adviserDetailId:guid}")]HttpRequestMessage req, TraceWriter log, string adviserDetailId)
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