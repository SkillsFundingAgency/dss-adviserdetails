using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Helpers;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests
{
    [TestFixture]
    public class PostAdviserDetailHttpTriggerTests
    {
        private const string ValidAdviserDetailId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPostAdviserDetailHttpTriggerService _postAdviserDetailHttpTriggerService;
        private Models.AdviserDetail _address;

        [SetUp]
        public void Setup()
        {
            _address = Substitute.For<Models.AdviserDetail>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/AdviserDetails/")
            };

            _log = Substitute.For<ILogger>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postAdviserDetailHttpTriggerService = Substitute.For<IPostAdviserDetailHttpTriggerService>();
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns("00000000001");
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction();

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailHasFailedValidation()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(_request).Returns(Task.FromResult(_address).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("adviser detail Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.AdviserDetail>(), true).Returns(validationResults);

            var result = await RunFunction();

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailRequestIsInvalid()
        {

            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(_request).Throws(new JsonException());

            var result = await RunFunction();

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAdviserDetailRecord()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(_request).Returns(Task.FromResult(_address).Result);

            _postAdviserDetailHttpTriggerService.CreateAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            var result = await RunFunction();

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeCreated_WhenRequestNotIsValid()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(_request).Returns(Task.FromResult(_address).Result);

            _postAdviserDetailHttpTriggerService.CreateAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            var result = await RunFunction();

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetAdviserDetailFromRequest<Models.AdviserDetail>(_request).Returns(Task.FromResult(_address).Result);

            _postAdviserDetailHttpTriggerService.CreateAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult(_address).Result);

            var result = await RunFunction();

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction()
        {
            return await PostAdviserDetailHttpTrigger.Function.PostAdviserDetailHttpTrigger.Run(
                _request, _log, _httpRequestMessageHelper, _validate, _postAdviserDetailHttpTriggerService).ConfigureAwait(false);
        }

    }
}