using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service
{
    public interface IGetAdviserDetailByIdHttpTriggerService
    {
        Task<Models.AdviserDetail> GetAdviserDetailByIdAsync(Guid adviserDetailId);
    }
}