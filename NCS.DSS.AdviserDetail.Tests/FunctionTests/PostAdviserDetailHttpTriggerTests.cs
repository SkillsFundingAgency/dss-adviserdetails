using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class PostAdviserDetailHttpTriggerTests
    {

        private const string ValidAdviserDetailId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        private IPostAdviserDetailHttpTriggerService _postAdviserDetailHttpTriggerService;
        private Models.AdviserDetail _adviserdetail;

        [SetUp]
        public void Setup()
        {
            _adviserdetail = Substitute.For<Models.AdviserDetail>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _postAdviserDetailHttpTriggerService = Substitute.For<IPostAdviserDetailHttpTriggerService>();

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
            _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetail>(_request).Returns(Task.FromResult(_adviserdetail).Result);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        
        
        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailHasFailedValidation()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Adviser Name is a required field") };
            _validate.ValidateResource(Arg.Any<Models.AdviserDetail>(),false).Returns(validationResults);

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.AdviserDetail>(_request).Throws(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAdviserDetailRecord()
        {
            _postAdviserDetailHttpTriggerService.CreateAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsBadRequestStatusCode_WhenRequestIsInValid()
        {
            _postAdviserDetailHttpTriggerService.CreateAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _postAdviserDetailHttpTriggerService.CreateAsync(Arg.Any<Models.AdviserDetail>()).Returns(Task.FromResult<Models.AdviserDetail>(_adviserdetail).Result);

            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserdetailId)
        {
            return await PostAdviserDetailHttpTrigger.Function.PostAdviserDetailHttpTrigger.Run(
                _request,
                _log,
                _resourceHelper,
                _postAdviserDetailHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }

    }
}