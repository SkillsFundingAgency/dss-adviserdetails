using System;
using System.Net;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using AdviserDetailFunction = NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Function;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class PatchAdviserDetailsHttpTriggerTests
    {

        private const string ValidAdviserDetailsId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private HttpRequest _request;
        private IValidate _validate;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IPatchAdviserDetailHttpTriggerService> _PatchAdviserDetailsHttpTriggerService;
        private Models.AdviserDetail _adviserDetail;
        private AdviserDetailPatch _adviserdetailPatch;
        private string _adviserDetailString;
        private AdviserDetailFunction.PatchAdviserDetailHttpTrigger _function;
        private Mock<ILogger<AdviserDetailFunction.PatchAdviserDetailHttpTrigger>> _logger;
        private Mock<IConvertToDynamic> _dynamicHelper;
        [SetUp]
        public void Setup()
        {
            _adviserDetail = new Models.AdviserDetail();
            _adviserdetailPatch = new AdviserDetailPatch();            

            _validate = new Validate();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _dynamicHelper = new Mock<IConvertToDynamic>();
            _PatchAdviserDetailsHttpTriggerService = new Mock<IPatchAdviserDetailHttpTriggerService>();
            _adviserDetailString = JsonConvert.SerializeObject(_adviserDetail);
            _logger = new Mock<ILogger<AdviserDetailFunction.PatchAdviserDetailHttpTrigger>>();
            _function = new AdviserDetailFunction.PatchAdviserDetailHttpTrigger(
                _PatchAdviserDetailsHttpTriggerService.Object, 
                _validate, 
                _loggerHelper.Object, 
                _httpRequestHelper.Object, 
                _logger.Object,
                _dynamicHelper.Object);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailsIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x=>x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeNoContent_WhenAdviserDetailsPatchCantBePatched()
        {
            // Arrange
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns((string)null);
            _httpRequestHelper.Setup(x=>x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateAdviserDetailRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.AdviserDetail>(null));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns(_adviserDetailString);
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.AdviserDetail>(null));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x=>x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetail));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);
            var responseResult = result as JsonResult;

            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string adviserdetailId)
        {
            return await _function.RunAsync(
                _request,
                adviserdetailId);
        }
    }
}