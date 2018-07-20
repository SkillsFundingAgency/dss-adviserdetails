using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.AdviserDetail.Cosmos.Helper;
using NCS.DSS.AdviserDetail.GetAdviserDetailByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests
{
    [TestFixture]
    public class GetAdviserDetailByIdHttpTriggerTests
    {
        private const string ValidAdviserDetailId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IGetAdviserDetailByIdHttpTriggerService _getAdviserDetailByIdHttpTriggerService;
        private Models.AdviserDetail _adviserDetail;

        [SetUp]
        public void Setup()
        {
            _adviserDetail = Substitute.For<Models.AdviserDetail>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/AdviserDetails/1e1a555c-9633-4e12-ab28-09ed60d51cb")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getAdviserDetailByIdHttpTriggerService = Substitute.For<IGetAdviserDetailByIdHttpTriggerService>();
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenAdviserDetailIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenAdviserDetailDoesNotExist()
        {
            _getAdviserDetailByIdHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Models.AdviserDetail>(null).Result);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAdviserDetailByIdHttpTrigger_ReturnsStatusCodeOk_WhenAdviserDetailExists()
        {
            _getAdviserDetailByIdHttpTriggerService.GetAdviserDetailByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(_adviserDetail).Result);

            // Act
            var result = await RunFunction(ValidAdviserDetailId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string adviserDetailId)
        {
            return await GetAdviserDetailByIdHttpTrigger.Function.GetAdviserDetailByIdHttpTrigger.Run(
                _request, _log, adviserDetailId, _resourceHelper, _getAdviserDetailByIdHttpTriggerService).ConfigureAwait(false);
        }

    }
}
