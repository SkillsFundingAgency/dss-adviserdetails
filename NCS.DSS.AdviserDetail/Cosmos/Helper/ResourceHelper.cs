using System;
using System.Collections.Generic;
using NCS.DSS.AdviserDetail.Cosmos.Provider;

namespace NCS.DSS.AdviserDetail.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

    }
}
