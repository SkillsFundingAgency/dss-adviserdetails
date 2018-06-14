using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.AdviserDetail.PutAdviserDetailHttpTrigger
{
    public static class PutAdviserDetailHttpTrigger
    {
        [Disable]
        [FunctionName("Put")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "AdviserDetails/{adviserDetailId}")]HttpRequestMessage req, TraceWriter log, string adviserDetailId)
        {
            log.Info("Put Adviser Detail C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Replaced Adviser Detail record with Id of : " + adviserDetailGuid)
            };
        }
    }
}