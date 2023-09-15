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
using Moq;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class PatchAdviserDetailsHttpTriggerTests
    {

        private const string ValidAdviserDetailsId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IPatchAdviserDetailHttpTriggerService> _PatchAdviserDetailsHttpTriggerService;
        private Models.AdviserDetail _adviserDetail;
        private AdviserDetailPatch _adviserdetailPatch;
        private string _adviserDetailString;
        private PatchAdviserDetailHttpTrigger.Function.PatchAdviserDetailHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _adviserDetail = new Models.AdviserDetail();
            _adviserdetailPatch = new AdviserDetailPatch();
            //_request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _PatchAdviserDetailsHttpTriggerService = new Mock<IPatchAdviserDetailHttpTriggerService>();
            _adviserDetailString = JsonConvert.SerializeObject(_adviserDetail);
            _function = new PatchAdviserDetailHttpTrigger.Function.PatchAdviserDetailHttpTrigger(
                _resourceHelper.Object, 
                _PatchAdviserDetailsHttpTriggerService.Object, 
                _validate, 
                _loggerHelper.Object, 
                _httpRequestHelper.Object, 
                _httpResponseMessageHelper, 
                _jsonHelper);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
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
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
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
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
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
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
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
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
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

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserNameRequestIsInValid()
        {
            // Arrange
            _adviserdetailPatch = new AdviserDetailPatch {  AdviserName = "<script>alert(1)</script>" };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetail));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var error = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(error.Contains("The field AdviserName must match the regular expression"));
        }

        [TestCase("<script>alert(1)</script>")]
        [TestCase("testing.email-address@test<script>.com")]
        [TestCase("@test.co.uk")]
        [TestCase("testing-email")]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserEmailddressRequestIsInValid(string emailAddress)
        {
            // Arrange
            _adviserdetailPatch = new AdviserDetailPatch { AdviserEmailAddress = emailAddress };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetail));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var error = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(error.Contains("The field AdviserEmailAddress must match the regular expression"));
        }

        [TestCase("testing.email-address@test.co.uk")]
        [TestCase("abcd.efgs2@jobs-22.co.uk")]
        [TestCase("testing@educationdevelopmenttrust.com")]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeOK_WhenAdviserEmailAddressRequestIsValid(string emailAddress)
        {
            // Arrange
            _adviserdetailPatch = new AdviserDetailPatch { AdviserEmailAddress = emailAddress };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetail));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert            
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [TestCase("123<script>alert(1)</script>")]
        [TestCase("o123456987")]
        [TestCase("12354<797")]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserContactNumberRequestIsInValid(string contactNumber)
        {
            // Arrange
            _adviserdetailPatch = new AdviserDetailPatch { AdviserContactNumber = contactNumber };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetail));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var error = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(error.Contains("The field AdviserContactNumber must match the regular expression"));
        }

        [TestCase("020 8315 1500")]
        [TestCase("0789456123")]
        [TestCase("07894 56123")]
        [TestCase("+44 7894 56123")]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeOK_WhenAdviserContactNumberRequestIsValid(string contactNumber)
        {
            // Arrange
            _adviserdetailPatch = new AdviserDetailPatch { AdviserContactNumber = contactNumber };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.GetAdviserDetailByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetailString));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(_adviserDetail));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<AdviserDetailPatch>(_request)).Returns(Task.FromResult(_adviserdetailPatch));
            _PatchAdviserDetailsHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<AdviserDetailPatch>())).Returns(_adviserDetailString);

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

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