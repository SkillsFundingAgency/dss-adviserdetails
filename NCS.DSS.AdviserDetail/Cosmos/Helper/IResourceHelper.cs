using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
    }
}