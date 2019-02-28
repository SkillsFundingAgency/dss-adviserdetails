using System;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Cosmos.Provider;

namespace NCS.DSS.AdviserDetail.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        public ResourceHelper(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var doesCustomerExist = await _documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

    }
}
