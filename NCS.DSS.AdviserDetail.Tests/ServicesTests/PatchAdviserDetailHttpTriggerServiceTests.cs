using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.ServicesTests
{
    [TestFixture]
    public class PatchAdviserDetailsHttpTriggerServiceTests
    {
        private IPatchAdviserDetailHttpTriggerService _adviserdetailPatchHttpTriggerService;
        private IAdviserDetailPatchService _adviserdetailPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.AdviserDetail _adviserdetail;
        private AdviserDetailPatch _adviserdetailPatch;
        private string _json;

        private readonly Guid _adviserdetailId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        private readonly Guid _interactionId = Guid.Parse("a06b29da-d949-4486-8b18-c0107dc8bae8");
        private readonly Guid _actionPlanId = Guid.Parse("679897e6-c16a-41ba-90c9-f4fbd0a9f666");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _adviserdetailPatchService = Substitute.For<IAdviserDetailPatchService>();
            _adviserdetailPatchHttpTriggerService = Substitute.For<PatchAdviserDetailHttpTriggerService>(_documentDbProvider, _adviserdetailPatchService);
            _adviserdetail = Substitute.For<Models.AdviserDetail>();
            _adviserdetailPatch = Substitute.For<AdviserDetailPatch>();
            _json = JsonConvert.SerializeObject(_adviserdetailPatch);
            _adviserdetailPatchService.Patch(_json, _adviserdetailPatch).Returns(_adviserdetail);
        }

        [Test]
        public void PatchAdviserDetailsHttpTriggerServiceTests_PatchResource_ReturnsNullWhenAdviserDetailJsonIsNullOrEmpty()
        {
            // Act
            var result = _adviserdetailPatchHttpTriggerService.PatchResource(null, Arg.Any<AdviserDetailPatch>());

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public void PatchAdviserDetailsHttpTriggerServiceTests_PatchResource_ReturnsNullWhenAdviserDetailPatchIsNullOrEmpty()
        {
            // Act
            var result = _adviserdetailPatchHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenAdviserDetailPatchServicePatchJsonIsNullOrEmpty()
        {
            _adviserdetailPatchService.Patch(Arg.Any<string>(), Arg.Any<AdviserDetailPatch>()).ReturnsNull();

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserdetail);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            _documentDbProvider.UpdateAdviserDetailAsync(Arg.Any<Models.AdviserDetail>()).ReturnsNull();

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserdetail);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateAdviserDetailAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserdetail);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchAdviserDetailsHttpTriggerServiceTests_UpdateCosmosAsync_ReturnsResourceWhenUpdated()
        {
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

            _documentDbProvider.UpdateAdviserDetailAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.UpdateCosmosAsync(_adviserdetail);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.AdviserDetail>(result);

        }

        [Test]
        public async Task  PatchAdviserDetailsHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetAdviserDetailsByIdToUpdateAsync(Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task  PatchAdviserDetailsHttpTriggerServiceTests_GetActionPlanForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetAdviserDetailsByIdToUpdateAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _adviserdetailPatchHttpTriggerService.GetAdviserDetailByIdAsync( Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}
