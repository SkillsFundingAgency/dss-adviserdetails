using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailHttpTrigger
{
    public static class GetAdviserDetailHttpTrigger
    {
        [FunctionName("Get")]
        [ResponseType(typeof(Models.AdviserDetail))]
        [Display(Name = "Get", Description = "Ability to return the adviser details for a given interaction.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "AdviserDetails")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Get Adviser Details C# HTTP trigger function processed a request.");

            var adviserDetailService = new GetAdviserDetailHttpTriggerService();
            var adviserDetails = await adviserDetailService.GetAdviserDetails();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(adviserDetails),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}