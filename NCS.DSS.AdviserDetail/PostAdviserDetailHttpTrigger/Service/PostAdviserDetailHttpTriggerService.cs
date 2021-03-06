﻿using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Cosmos.Provider;

namespace NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service
{
    public class PostAdviserDetailHttpTriggerService : IPostAdviserDetailHttpTriggerService
    {
        public async Task<Models.AdviserDetail> CreateAsync(Models.AdviserDetail adviserDetail)
        {
            if (adviserDetail == null)
                return null;

            adviserDetail.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateAdviserDetailAsync(adviserDetail);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : (Guid?)null;
        }
    }
}