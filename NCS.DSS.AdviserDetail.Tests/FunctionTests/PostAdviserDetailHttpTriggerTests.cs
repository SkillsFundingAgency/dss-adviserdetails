using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.Cosmos.Provider;
using NCS.DSS.AdviserDetail.PostAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using NCS.DSS.AdviserDetails.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IPostAdviserDetailHttpTriggerService> _postAdviserDetailHttpTriggerService;
        private Mock<IDocumentDBProvider> _provider;
        private Models.AdviserDetail _adviserdetail;
        private PostAdviserDetailHttpTrigger.Function.PostAdviserDetailHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _adviserdetail = new Models.AdviserDetail() { AdviserName = "testing" };
            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _request.Headers.Add(TouchpointIdHeaderParamKey, TouchpointIdHeaderParamValue);
            _request.Headers.Add(ApimUrlHeaderParameterKey, ApimUrlHeaderParameterValue);
            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _provider = new Mock<IDocumentDBProvider>();
            _postAdviserDetailHttpTriggerService = new Mock<IPostAdviserDetailHttpTriggerService>();
            _function = new PostAdviserDetailHttpTrigger.Function.PostAdviserDetailHttpTrigger(
                _resourceHelper.Object,
                _postAdviserDetailHttpTriggerService.Object,
                _validate,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult<Models.AdviserDetail>(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(_adviserdetail));
            var validate = new Mock<IValidate>();
            List<System.ComponentModel.DataAnnotations.ValidationResult> err = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            err.Add(new System.ComponentModel.DataAnnotations.ValidationResult("some error"));
            validate.Setup(x => x.ValidateResource(It.IsAny<IAdviserDetail>(), It.IsAny<bool>())).Returns(err);
            _function = new PostAdviserDetailHttpTrigger.Function.PostAdviserDetailHttpTrigger(
                _resourceHelper.Object,
                _postAdviserDetailHttpTriggerService.Object,
                validate.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserDetailRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult<Models.AdviserDetail>(null));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(null));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }



        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAdviserDetailRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(new Models.AdviserDetail() { AdviserName = "some name" }));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult<Models.AdviserDetail>(null));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAdviserDetailHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        [TestCase("testing<script>-address@test.com")]
        [TestCase("testing.email-address@test")]
        [TestCase("@test.co.uk")]
        [TestCase("testing-email")]
        public async Task PostAdviserDetailsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserEmailAddressRequestIsInValid(string emailAddress)
        {
            // Arrange
            _adviserdetail = new Models.AdviserDetail { AdviserName = "Test Adviser", AdviserEmailAddress = emailAddress };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            var error = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(error.Contains("The field AdviserEmailAddress must match the regular expression"));
        }

        [TestCase("testing.email-address@test.co.uk")]
        [TestCase("abcd.efgs2@jobs-22.co.uk")]
        [TestCase("testing@educationdevelopmenttrust.com")]
        public async Task PostAdviserDetailsHttpTrigger_ReturnsStatusCodeOK_WhenAdviserEmailAddressRequestIsValid(string emailAddress)
        {
            // Arrange
            _adviserdetail = new Models.AdviserDetail { AdviserName = "Test Adviser", AdviserEmailAddress = emailAddress };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        [TestCase("123<script>alert(1)</script>")]
        [TestCase("o123456987")]
        [TestCase("12354<797")]
        public async Task PostAdviserDetailsHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAdviserContactNumberRequestIsInValid(string contactNumber)
        {
            // Arrange
            _adviserdetail = new Models.AdviserDetail { AdviserContactNumber = contactNumber };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

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
        public async Task PostAdviserDetailsHttpTrigger_ReturnsStatusCodeOK_WhenAdviserContactNumberRequestIsValid(string contactNumber)
        {
            // Arrange
            _adviserdetail = new Models.AdviserDetail { AdviserName = "Test Adviser", AdviserContactNumber = contactNumber };

            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AdviserDetail>(_request)).Returns(Task.FromResult(_adviserdetail));
            _postAdviserDetailHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.AdviserDetail>())).Returns(Task.FromResult(_adviserdetail));

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert            
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserdetailId)
        {
            return await _function.Run(
                _request,
                _log.Object).ConfigureAwait(false);
        }
    }
}