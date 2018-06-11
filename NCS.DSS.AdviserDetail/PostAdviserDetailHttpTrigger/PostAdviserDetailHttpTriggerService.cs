using System;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger
{
    public class PostAdviserDetailHttpTriggerService
    {
        public Guid? Create(Models.AdviserDetail adviserDetail)
        {
            if (adviserDetail == null)
                return null;

            return Guid.NewGuid();
;        }
    }
}