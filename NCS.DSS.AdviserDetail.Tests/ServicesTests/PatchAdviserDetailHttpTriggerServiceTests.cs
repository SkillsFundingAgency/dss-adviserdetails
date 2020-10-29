using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using Newtonsoft.Json;
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
    public class PatchAdviserDetailsHttpTriggerServiceTests
    {
        private IPatchAdviserDetailHttpTriggerService _adviserdetailPatchHttpTriggerService;
        private Mock<IAdviserDetailPatchService> _adviserdetailPatchService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private Models.AdviserDetail _adviserDetail;
        private AdviserDetailPatch _adviserDetailPatch;
        private string _json;
        private string _adviserDetailString;

        private readonly Guid _adviserDetailId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");


        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _adviserdetailPatchService = new Mock<IAdviserDetailPatchService>();
            _adviserdetailPatchHttpTriggerService = new PatchAdviserDetailHttpTriggerService(_documentDbProvider.Object, _adviserdetailPatchService.Object);
            _adviserDetail = new Models.AdviserDetail();
            _adviserDetailPatch = new AdviserDetailPatch();
            _json = JsonConvert.SerializeObject(_adviserDetailPatch);
            _adviserDetailString = JsonConvert.SerializeObject(_adviserDetail);
        }

        [Test]
        public void PatchAdviserDetailsHttpTriggerServiceTests_PatchResource_ReturnsNullWhenAdviserDetailJsonIsNullOrEmpty()
        {
            // Act
            var result = _adviserdetailPatchHttpTriggerService.PatchResource(null, It.IsAny<AdviserDetailPatch>());

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public void PatchAdviserDetailsHttpTriggerServiceTests_PatchResource_ReturnsNullWhenAdviserDetailPatchIsNullOrEmpty()
        {
            // Act
            var result = _adviserdetailPatchHttpTriggerService.PatchResource(It.IsAny<string>(), null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenAdviserDetailPatchServicePatchJsonIsNullOrEmpty()
        {
            // Arrange
            _adviserdetailPatchService.Setup(x=>x.Patch(It.IsAny<string>(), It.IsAny<AdviserDetailPatch>())).Returns<string>(null);

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserDetailString, _adviserDetailId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.UpdateAdviserDetailAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<ResourceResponse<Document>>(null));

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserDetailString, _adviserDetailId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.CreateAdviserDetailAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(new ResourceResponse<Document>(null)));

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserDetailString, _adviserDetailId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.Setup(x=>x.UpdateAdviserDetailAsync(_adviserDetailString, _adviserDetailId)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserDetailString, _adviserDetailId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.AdviserDetail>(result);

        }

        [Test]
        public async Task PatchAdviserDetailsHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetAdviserDetailsByIdToUpdateAsync(It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.GetAdviserDetailByIdAsync(It.IsAny<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            // Arrange
            _documentDbProvider.Setup(x=>x.GetAdviserDetailsByIdToUpdateAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_json));

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.GetAdviserDetailByIdAsync(It.IsAny<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}
