using System;
using DFC.JSON.Standard;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.ServicesTests
{

    [TestFixture]
    public class AdviserDetailPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private AdviserDetailPatchService _adviserdetailPatchService;
        private AdviserDetailPatch _adviserdetailPatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = Substitute.For<JsonHelper>();
            _adviserdetailPatchService = Substitute.For<AdviserDetailPatchService>(_jsonHelper);
            _adviserdetailPatch = Substitute.For<AdviserDetailPatch>();

            _json = JsonConvert.SerializeObject(_adviserdetailPatch);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_ReturnsNull_WhenAdviserDetailPatchIsNull()
        {
            var result = _adviserdetailPatchService.Patch(string.Empty, Arg.Any<AdviserDetailPatch>());

            // Assert
            Assert.IsNull(result);
        }

        
        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserContactNumberIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new AdviserDetailPatch {  AdviserContactNumber = "1111" };

            var AdviserDetail = _adviserdetailPatchService.Patch(_json, diversityPatch);

            var AdviserDetailType = AdviserDetail.AdviserContactNumber;
            
            // Assert
            Assert.AreEqual("1111", AdviserDetailType);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserEmailAddressIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new AdviserDetailPatch {  AdviserEmailAddress = "1@1.com" };

            var AdviserDetail = _adviserdetailPatchService.Patch(_json, diversityPatch);

            var AdviserDetailClaimedDate = AdviserDetail.AdviserEmailAddress;

            // Assert
            Assert.AreEqual("1@1.com", AdviserDetailClaimedDate);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserNameIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new Models.AdviserDetailPatch {  AdviserName = "name" };

            var AdviserDetail = _adviserdetailPatchService.Patch(_json, diversityPatch);

            var AdviserDetailEffectiveDate = AdviserDetail.AdviserName;

            // Assert
            Assert.AreEqual("name", AdviserDetailEffectiveDate);
        }


        [Test]
        public void AdviserDetailPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new AdviserDetailPatch { LastModifiedDate = DateTime.MaxValue };

            var AdviserDetail = _adviserdetailPatchService.Patch(_json, diversityPatch);

            var lastModifiedDate = AdviserDetail.LastModifiedDate;
            
            // Assert
            Assert.AreEqual(DateTime.MaxValue, lastModifiedDate);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            var diversityPatch = new AdviserDetailPatch { LastModifiedTouchpointId = "0000000111" };

            var AdviserDetail = _adviserdetailPatchService.Patch(_json, diversityPatch);

            var lastModifiedTouchpointId = AdviserDetail.LastModifiedTouchpointId;

            // Assert
            Assert.AreEqual("0000000111", lastModifiedTouchpointId);
        }
        
    }
}