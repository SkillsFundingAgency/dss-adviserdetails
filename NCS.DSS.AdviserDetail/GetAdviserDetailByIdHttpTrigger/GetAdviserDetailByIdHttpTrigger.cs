using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger
{
    public static class GetAdviserDetailByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [Display(Name = "Get", Description = "Ability to return the adviser details for a given interaction.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AdviserDetails/{adviserDetailId}")]HttpRequestMessage req, TraceWriter log, string adviserDetailId)
        {
            log.Info("Get Adviser Detail By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(adviserDetailId, out var adviserDetailGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(adviserDetailId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var adviserDetailService = new GetAdviserDetailByIdHttpTriggerService();
            var adviserDetail = await adviserDetailService.GetAdviserDetail(adviserDetailGuid);

            if (adviserDetail == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        "Unable to find Adviser Detail record with Id of : " + adviserDetailGuid)
                };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(adviserDetail),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}