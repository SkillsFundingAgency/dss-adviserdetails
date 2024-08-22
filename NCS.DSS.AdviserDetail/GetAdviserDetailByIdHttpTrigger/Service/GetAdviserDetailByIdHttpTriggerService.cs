using NCS.DSS.AdviserDetail.Cosmos.Provider;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service
{
    public class GetAdviserDetailByIdHttpTriggerService : IGetAdviserDetailByIdHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetAdviserDetailByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }
        public async Task<Models.AdviserDetail> GetAdviserDetailAsync(Guid adviserDetailId)
        {
            var AdviserDetail = await _documentDbProvider.GetAdviserDetailByIdAsync(adviserDetailId);

            return AdviserDetail;
        }
    }
}