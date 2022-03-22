using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class GetAdviserDetailByIdHttpTriggerTests
    {

        private const string ValidAdviserDetailId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IGetAdviserDetailByIdHttpTriggerService> _GetAdviserDetailByIdHttpTriggerService;
        private Models.AdviserDetail _adviserdetail;
        private GetAdviserDetailByIdHttpTrigger.Function.GetAdviserDetailByIdHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _adviserdetail = new Models.AdviserDetail();
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _resourceHelper = new Mock<IResourceHelper>();
            _GetAdviserDetailByIdHttpTriggerService = new Mock<IGetAdviserDetailByIdHttpTriggerService>();
            _function = new GetAdviserDetailByIdHttpTrigger.Function.GetAdviserDetailByIdHttpTrigger(
                _resourceHelper.Object, 
                _GetAdviserDetailByIdHttpTriggerService.Object, 
                _loggerHelper.Object, 
                _httpRequestHelper.Object, 
                _httpResponseMessageHelper, 
                _jsonHelper);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }


        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubcontractorIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeOk_WhenAdviserDetailDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _GetAdviserDetailByIdHttpTriggerService.Setup(x=>x.GetAdviserDetailAsync(It.IsAny<Guid>())).Returns(Task.FromResult<Models.AdviserDetail>(null));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeOk_WhenAdviserDetailExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _GetAdviserDetailByIdHttpTriggerService.Setup(x=>x.GetAdviserDetailAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserdetailId)
        {
            return await _function.Run(
                _request,
                _log.Object,
                adviserdetailId).ConfigureAwait(false);
        }
    }
}