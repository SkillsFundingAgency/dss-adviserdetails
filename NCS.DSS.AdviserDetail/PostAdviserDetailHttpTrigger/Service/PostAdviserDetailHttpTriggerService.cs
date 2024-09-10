using NCS.DSS.AdviserDetail.Cosmos.Provider;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service
{
    public class PostAdviserDetailHttpTriggerService : IPostAdviserDetailHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public PostAdviserDetailHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.AdviserDetail> CreateAsync(Models.AdviserDetail AdviserDetail)
        {
            if (AdviserDetail == null)
                return null;

            AdviserDetail.SetDefaultValues();

            var response = await _documentDbProvider.CreateAdviserDetailAsync(AdviserDetail);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

    }
}