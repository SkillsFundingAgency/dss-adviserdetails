using System;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service
{
    public class PostAdviserDetailHttpTriggerService : IPostAdviserDetailHttpTriggerService
    {
        public Guid? Create(Models.AdviserDetail adviserDetail)
        {
            if (adviserDetail == null)
                return null;

            return Guid.NewGuid();
;        }
    }
}