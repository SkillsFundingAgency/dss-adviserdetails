using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetAdviserDetailFromRequest<T>(HttpRequestMessage req);
    }
}