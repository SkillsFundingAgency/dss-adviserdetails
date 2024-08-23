using Moq;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Tests.ServicesTests
{
    [TestFixture]
    public class GetAdviserDetailByIdHttpTriggerServiceTests
    {
        private IGetAdviserDetailByIdHttpTriggerService _adviserdetailHttpTriggerService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Models.AdviserDetail _adviserdetail;
        private readonly Guid _adviserdetailId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _adviserdetailHttpTriggerService = new GetAdviserDetailByIdHttpTriggerService(_documentDbProvider.Object);
            _adviserdetail = new Models.AdviserDetail();
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTriggerServiceTests_GetAdviserDetailForCustomerAsyncc_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Models.AdviserDetail>(null));

            // Act
            var result = await _adviserdetailHttpTriggerService.GetAdviserDetailAsync(_adviserdetailId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTriggerServiceTests_GetAdviserDetailForCustomerAsync_ReturnsResource()
        {
            // Arrange
            _documentDbProvider.Setup(x => x.GetAdviserDetailByIdAsync(_adviserdetailId)).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await _adviserdetailHttpTriggerService.GetAdviserDetailAsync(_adviserdetailId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.AdviserDetail>());
        }
    }
}
