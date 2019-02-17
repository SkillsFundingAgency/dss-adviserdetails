using System;
using System.Threading.Tasks;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.ServicesTests
{
    [TestFixture]
    public class GetAdviserDetailByIdHttpTriggerServiceTests
    {
        private IGetAdviserDetailByIdHttpTriggerService _adviserdetailHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.AdviserDetail _adviserdetail;
        private readonly Guid _adviserdetailId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _adviserdetailHttpTriggerService = Substitute.For<GetAdviserDetailByIdHttpTriggerService>(_documentDbProvider);
            _adviserdetail = Substitute.For<Models.AdviserDetail>();
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTriggerServiceTests_GetAdviserDetailForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            // Act
            var result = await _adviserdetailHttpTriggerService.GetAdviserDetailAsync(_adviserdetailId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTriggerServiceTests_GetAdviserDetailForCustomerAsync_ReturnsResource()
        {
            _documentDbProvider.GetAdviserDetailByIdAsync(_adviserdetailId).Returns(Task.FromResult(_adviserdetail).Result);

            // Act
            var result = await _adviserdetailHttpTriggerService.GetAdviserDetailAsync(_adviserdetailId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.AdviserDetail>(result);
        }
    }
}
