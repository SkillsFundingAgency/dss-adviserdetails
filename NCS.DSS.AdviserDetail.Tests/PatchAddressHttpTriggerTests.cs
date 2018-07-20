using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests
{
    [TestFixture]
    public class PatchAdviserDetailHttpTriggerTests
    {
        private const string ValidAdviserDetailId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPatchAdviserDetailHttpTriggerService _patchAdviserDetailHttpTriggerService;
        private Models.AdviserDetail _address;
        private Models.AdviserDetailPatch _addressPatch;

        [SetUp]
        public void Setup()
        {
            _address = Substitute.For<Models.AdviserDetail>();
            _addressPatch = Substitute.For<Models.AdviserDetailPatch>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"AdviserDetails/1e1a555c-9633-4e12-ab28-09ed60d51cb3")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _patchAdviserDetailHttpTriggerService = Substitute.For<IPatchAdviserDetailHttpTriggerService>();
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailIdIsInvalid()
        {
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailHasFailedValidation()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetailPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("address Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.AdviserDetailPatch>()).Returns(validationResults);

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(_request).Throws(new JsonSerializationException());

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeNoContent_WhenAdviserDetailDoesNotExist()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetailPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _patchAdviserDetailHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateAdviserDetailRecord()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetailPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _patchAdviserDetailHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Models.AdviserDetail>(_address).Result);

            _patchAdviserDetailHttpTriggerService.UpdateAsync(Arg.Any<Models.AdviserDetail>(), Arg.Any<Models.AdviserDetailPatch>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsNotValid()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetailPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _patchAdviserDetailHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_address).Result);

            _patchAdviserDetailHttpTriggerService.UpdateAsync(Arg.Any<Models.AdviserDetail>(), Arg.Any<Models.AdviserDetailPatch>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetailPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _patchAdviserDetailHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_address).Result);

            _patchAdviserDetailHttpTriggerService.UpdateAsync(Arg.Any<Models.AdviserDetail>(), Arg.Any<Models.AdviserDetailPatch>()).Returns(Task.FromResult(_address).Result);

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserDetailId)
        {
            return await PatchAdviserDetailHttpTrigger.Function.PatchAdviserDetailHttpTrigger.Run(
                _request, _log, adviserDetailId, _resourceHelper, _httpRequestMessageHelper, _validate, _patchAdviserDetailHttpTriggerService).ConfigureAwait(false);
        }

    }
}