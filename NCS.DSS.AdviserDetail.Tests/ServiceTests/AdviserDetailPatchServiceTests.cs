using System;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.ServiceTests
{
    [TestFixture]
    public class AdviserDetailPatchServiceTests
    {
        private IAdviserDetailPatchService _adviserDetailPatchService;
        private AdviserDetailPatch _adviserDetailPatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _adviserDetailPatchService = Substitute.For<AdviserDetailPatchService>();
            _adviserDetailPatch = Substitute.For<AdviserDetailPatch>();

            _json = JsonConvert.SerializeObject(_adviserDetailPatch);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_ReturnsNull_WhenAdviserDetailPatchIsNull()
        {
            var result = _adviserDetailPatchService.Patch(string.Empty, Arg.Any<AdviserDetailPatch>());

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserContactNumberIsUpdated_WhenPatchIsCalled()
        {
            var adviserDetailPatch = new AdviserDetailPatch {AdviserContactNumber = "1111"};

            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);

            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert
            Assert.AreEqual("1111", adviserDetail.AdviserContactNumber);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserEmailAddressIsUpdated_WhenPatchIsCalled()
        {
            var adviserDetailPatch = new AdviserDetailPatch {AdviserEmailAddress = "1@1.com"};

            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);

            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert
            Assert.AreEqual("1@1.com", adviserDetail.AdviserEmailAddress);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserNameIsUpdated_WhenPatchIsCalled()
        {
            var adviserDetailPatch = new Models.AdviserDetailPatch {AdviserName = "name"};

            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);

            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert
            Assert.AreEqual("name", adviserDetail.AdviserName);
        }


        [Test]
        public void AdviserDetailPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var adviserDetailPatch = new AdviserDetailPatch {LastModifiedDate = DateTime.MaxValue};

            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);

            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, adviserDetail.LastModifiedDate);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var adviserDetailPatch = new AdviserDetailPatch {LastModifiedTouchpointId = "0000000111"};

            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);

            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert
            Assert.AreEqual("0000000111", adviserDetail.LastModifiedTouchpointId);
        }

    }
}
