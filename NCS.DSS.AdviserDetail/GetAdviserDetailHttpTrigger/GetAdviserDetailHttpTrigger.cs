using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using NCS.DSS.AdviserDetail.Annotations;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailHttpTrigger
{
    public static class GetAdviserDetailHttpTrigger
    {
        [FunctionName("Get")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Adviser Details found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Adviser Details do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return a list of advisers that have had interactions with a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customer/{customerId}/AdviserDetails")]HttpRequestMessage req, TraceWriter log, string customerId)
        {
            log.Info("Get Adviser Details C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(customerId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var adviserDetailService = new GetAdviserDetailHttpTriggerService();
            var adviserDetails = await adviserDetailService.GetAdviserDetailIdsForCustomer(customerGuid);
            
            if (!adviserDetails.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(customerGuid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(adviserDetails),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}