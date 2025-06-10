using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Tests.ServicesTests
{
    [TestFixture]
    public class PostAdviserDetailHttpTriggerServiceTests
    {
        private IPostAdviserDetailHttpTriggerService _postAdviserDetailHttpTriggerService;
        private Mock<ICosmosDBProvider> _documentDbProvider;
        private Models.AdviserDetail _adviserdetail;

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<ICosmosDBProvider>();
            _postAdviserDetailHttpTriggerService = new PostAdviserDetailHttpTriggerService(_documentDbProvider.Object);
            _adviserdetail = new Models.AdviserDetail();
        }

        [Test]
        public async Task PostAdviserDetailHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenOutcomeJsonIsNullOrEmpty()
        {
            // Act
            var result = await _postAdviserDetailHttpTriggerService.CreateAsync(null);

            // Assert            
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostAdviserDetailHttpTriggerServiceTests_CreateAsync_ReturnsResourceWhenUpdated()
        {
            //Arrange
            var resourceResponse = new Mock<ItemResponse<Models.AdviserDetail>>();
            resourceResponse.Setup(x => x.Resource).Returns(_adviserdetail);
            resourceResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Created);
           
            _documentDbProvider.Setup(x => x.CreateAdviserDetailAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(resourceResponse.Object));

            // Act
            var result = await _postAdviserDetailHttpTriggerService.CreateAsync(_adviserdetail);

            // Assert                        
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.AdviserDetail>());

        }
    }
}
