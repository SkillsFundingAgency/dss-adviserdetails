using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetAdviserDetailFromRequest<T>(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
