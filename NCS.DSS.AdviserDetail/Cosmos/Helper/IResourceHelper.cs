using System;

namespace NCS.DSS.AdviserDetail.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
    }
}