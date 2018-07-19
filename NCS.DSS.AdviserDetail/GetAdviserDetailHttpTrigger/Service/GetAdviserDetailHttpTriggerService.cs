using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.GetAdviserDetailHttpTrigger.Service
{
    public class GetAdviserDetailHttpTriggerService : IGetAdviserDetailHttpTriggerService
    {
        public async Task<List<Models.AdviserDetail>> GetAdviserDetails()
        {
            var result = CreateTempAdviserDetails();
            return await Task.FromResult(result);
        }

        public async Task<List<Guid>> GetAdviserDetailIdsForCustomer(Guid customerId)
        {
            var listOfAdviserDetailIds = new List<Guid> {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
            return await Task.FromResult(listOfAdviserDetailIds);
        }

        public List<Models.AdviserDetail> CreateTempAdviserDetails()
        {
            var adviserDetailList = new List<Models.AdviserDetail>
            {
                new Models.AdviserDetail
                {
                    AdviserDetailId = Guid.Parse("393c069b-f55f-41a9-b2bb-ea254cd8492f"),
                    AdviserName = "Thor Odinson",
                    AdviserEmailAddress = "Thor@Asgard.com",
                    AdviserContactNumber = "00112233445",
                    LastModifiedDate = DateTime.Today.AddYears(-1),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.AdviserDetail
                {
                    AdviserDetailId = Guid.Parse("3d102b55-1581-4e68-a74c-13a8e35abbf1"),
                    AdviserName = "Hulk",
                    AdviserEmailAddress = "Hulk@Marvel.com",
                    AdviserContactNumber = "00223344556",
                    LastModifiedDate = DateTime.Today.AddYears(-2),
                    LastModifiedTouchpointId = Guid.NewGuid()
                },
                new Models.AdviserDetail
                {
                    AdviserDetailId = Guid.Parse("09bb2c42-7cc7-4a0a-9435-8fa906577937"),
                    AdviserName = "Black Panther",
                    AdviserEmailAddress = "Panter@Wakanda.com",
                    AdviserContactNumber = "00334455667",
                    LastModifiedDate = DateTime.Today.AddYears(-3),
                    LastModifiedTouchpointId = Guid.NewGuid()
                }
            };

            return adviserDetailList;
        }


    }
}