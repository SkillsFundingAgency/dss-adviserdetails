using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service
{
    public interface IPostAdviserDetailHttpTriggerService
    {
        Task<Models.AdviserDetail> CreateAsync(Models.AdviserDetail adviserDetail);
    }
}