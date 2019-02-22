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
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using NCS.DSS.AdviserDetail.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.FunctionTests
{
    [TestFixture]
    public class PatchAdviserDetailsHttpTriggerTests
    {

        private const string ValidAdviserDetailsId = "cff8080e-1da2-42bd-9b63-8f235aad9d86";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IPatchAdviserDetailHttpTriggerService _PatchAdviserDetailsHttpTriggerService;
        private Models.AdviserDetail _adviserDetail;
        private AdviserDetailPatch _adviserdetailPatch;
        private string _adviserDetailString;

        [SetUp]
        public void Setup()
        {
            _adviserDetail = Substitute.For<Models.AdviserDetail>();
            _adviserdetailPatch = Substitute.For<AdviserDetailPatch>();

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
            _PatchAdviserDetailsHttpTriggerService = Substitute.For<IPatchAdviserDetailHttpTriggerService>();
            _adviserDetailString = JsonConvert.SerializeObject(_adviserDetail);

            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:7071/");
            _httpRequestHelper.GetResourceFromRequest<AdviserDetailPatch>(_request).Returns(Task.FromResult(_adviserdetailPatch).Result);
            _PatchAdviserDetailsHttpTriggerService.PatchResource(Arg.Any<string>(), _adviserdetailPatch).Returns(_adviserDetailString);

        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string) null);

            _httpResponseMessageHelper
                .BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailsIdIsInvalid()
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
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeNoContent_WhenAdviserDetailsPatchCantBePatched()
        {
            _PatchAdviserDetailsHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_adviserDetailString).Result);

            _PatchAdviserDetailsHttpTriggerService.PatchResource(Arg.Any<string>(), Arg.Any<Models.AdviserDetailPatch>()).Returns((string)null);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateAdviserDetailRecord()
        {
            _PatchAdviserDetailsHttpTriggerService
                .GetAdviserDetailByIdAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(_adviserDetailString).Result);

            _PatchAdviserDetailsHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(),Arg.Any<Guid>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            _PatchAdviserDetailsHttpTriggerService
                .GetAdviserDetailByIdAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(_adviserDetailString).Result);

            _PatchAdviserDetailsHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAdviserDetailsHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _PatchAdviserDetailsHttpTriggerService
                .GetAdviserDetailByIdAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(_adviserDetailString).Result);
            
            _PatchAdviserDetailsHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult(_adviserDetail).Result);

            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

            var result = await RunFunction(ValidAdviserDetailsId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserdetailId)
        {
            return await PatchAdviserDetailHttpTrigger.Function.PatchAdviserDetailHttpTrigger.Run(
                _request,
                _log,
                adviserdetailId,
                _resourceHelper,
                _PatchAdviserDetailsHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }
    }
}