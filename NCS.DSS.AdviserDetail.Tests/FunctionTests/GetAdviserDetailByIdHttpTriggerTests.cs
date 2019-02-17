using System;
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
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class GetAdviserDetailByIdHttpTriggerTests
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
        private IGetAdviserDetailByIdHttpTriggerService _GetAdviserDetailByIdHttpTriggerService;
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

            _GetAdviserDetailByIdHttpTriggerService = Substitute.For<IGetAdviserDetailByIdHttpTriggerService>();
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
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
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailIdIsInvalid()
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
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeOk_WhenAdviserDetailDoesNotExist()
        {
            _GetAdviserDetailByIdHttpTriggerService.GetAdviserDetailAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeOk_WhenAdviserDetailExists()
        {

            _GetAdviserDetailByIdHttpTriggerService.GetAdviserDetailAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_adviserdetail).Result);

            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserdetailId)
        {
            return await GetAdviserDetailByIdHttpTrigger.Function.GetAdviserDetailByIdHttpTrigger.Run(
                _request,
                _log,
                adviserdetailId,
                _resourceHelper,
                _GetAdviserDetailByIdHttpTriggerService,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }

    }
}