using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.AdviserDetail.AdviserDetailChangeFeedTrigger.Service
{
    public interface IAdviserDetailChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
