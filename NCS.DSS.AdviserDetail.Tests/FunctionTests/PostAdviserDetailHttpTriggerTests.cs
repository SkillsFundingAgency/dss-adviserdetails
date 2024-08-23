using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using NCS.DSS.AdviserDetails.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AdviserDetailFunction = NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Function;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class PostAdviserDetailHttpTriggerTests
    {

        private const string ValidAdviserDetailId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string TouchpointIdHeaderParamKey = "touchpointId";
        private const string ApimUrlHeaderParameterKey = "apimurl";
        private string ApimUrlHeaderParameterValue = "http://localhost:7071/";
        private string TouchpointIdHeaderParamValue = "9000000000";
        private HttpRequest _request;
        private IValidate _validate;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IPostAdviserDetailHttpTriggerService> _postAdviserDetailHttpTriggerService;
        private Models.AdviserDetail _adviserdetail;
        private AdviserDetailFunction.PostAdviserDetailHttpTrigger _function;
        private Mock<ILogger<AdviserDetailFunction.PostAdviserDetailHttpTrigger>> _logger;
        private Mock<IConvertToDynamic> _dynamicHelper;
        [SetUp]
        public void Setup()
        {
            _adviserdetail = new Models.AdviserDetail() { AdviserName = "testing" };
            _request = new DefaultHttpContext().Request;
            _request.Headers.Add(TouchpointIdHeaderParamKey, TouchpointIdHeaderParamValue);
            _request.Headers.Add(ApimUrlHeaderParameterKey, ApimUrlHeaderParameterValue);
            _validate = new Validate();
            _dynamicHelper = new Mock<IConvertToDynamic>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _postAdviserDetailHttpTriggerService = new Mock<IPostAdviserDetailHttpTriggerService>();
            _logger = new Mock<ILogger<AdviserDetailFunction.PostAdviserDetailHttpTrigger>>();
            _function = new AdviserDetailFunction.PostAdviserDetailHttpTrigger(
                _postAdviserDetailHttpTriggerService.Object,
                _validate,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _logger.Object,
                _dynamicHelper.Object);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult<Models.AdviserDetail>(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(_adviserdetail));
            var validate = new Mock<IValidate>();
            List<System.ComponentModel.DataAnnotations.ValidationResult> err = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            err.Add(new System.ComponentModel.DataAnnotations.ValidationResult("some error"));
            validate.Setup(x => x.ValidateResource(It.IsAny<IAdviserDetail>(), It.IsAny<bool>())).Returns(err);
            _function = new AdviserDetailFunction.PostAdviserDetailHttpTrigger(
                _postAdviserDetailHttpTriggerService.Object,
                validate.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _logger.Object,
                _dynamicHelper.Object);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult<Models.AdviserDetail>(null));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(null));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityResult>());
        }


        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAdviserDetailRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(new Models.AdviserDetail() { AdviserName = "some name" }));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(null));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string adviserdetailId)
        {
            return await _function.RunAsync(
                _request);
        }
    }
}