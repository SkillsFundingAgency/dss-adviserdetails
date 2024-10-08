﻿using DFC.JSON.Standard;
using Moq;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetail.PatchAdviserDetailHttpTrigger.Service;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

namespace NCS.DSS.AdviserDetail.Tests.ServicesTests
{

    [TestFixture]
    public class AdviserDetailPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private AdviserDetailPatchService _adviserDetailPatchService;
        private AdviserDetailPatch _adviserDetailPatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = new JsonHelper();
            _adviserDetailPatchService = new AdviserDetailPatchService(_jsonHelper);
            _adviserDetailPatch = new AdviserDetailPatch();
            _json = JsonConvert.SerializeObject(_adviserDetailPatch);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_ReturnsNull_WhenAdviserDetailPatchIsNull()
        {
            // Act
            var result = _adviserDetailPatchService.Patch(string.Empty, It.IsAny<AdviserDetailPatch>());

            // Assert            
            Assert.That(result, Is.Null);
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserContactNumberIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var adviserDetailPatch = new AdviserDetailPatch { AdviserContactNumber = "1111" };

            // Act
            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);
            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert            
            Assert.That(adviserDetail.AdviserContactNumber, Is.EqualTo("1111"));
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserEmailAddressIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var adviserDetailPatch = new AdviserDetailPatch { AdviserEmailAddress = "1@1.com" };

            // Act
            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);
            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert            
            Assert.That(adviserDetail.AdviserEmailAddress, Is.EqualTo("1@1.com"));
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckAdviserDetailAdviserNameIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var adviserDetailPatch = new Models.AdviserDetailPatch { AdviserName = "name" };

            // Act
            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);
            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert            
            Assert.That(adviserDetail.AdviserName, Is.EqualTo("name"));
        }


        [Test]
        public void AdviserDetailPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var adviserDetailPatch = new AdviserDetailPatch { LastModifiedDate = DateTime.MaxValue };

            // Act
            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);
            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert            
            Assert.That(adviserDetail.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckLastModifiedByUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var adviserDetailPatch = new AdviserDetailPatch { LastModifiedTouchpointId = "0000000111" };

            // Act
            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);
            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert            
            Assert.That(adviserDetail.LastModifiedTouchpointId, Is.EqualTo("0000000111"));
        }

        [Test]
        public void AdviserDetailPatchServiceTests_CheckSubcontractorIdUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var adviserDetailPatch = new AdviserDetailPatch { SubcontractorId = "0000000111" };

            // Act
            var patchedAdviserDetail = _adviserDetailPatchService.Patch(_json, adviserDetailPatch);
            var adviserDetail = JsonConvert.DeserializeObject<Models.AdviserDetail>(patchedAdviserDetail);

            // Assert            
            Assert.That(adviserDetail.SubcontractorId, Is.EqualTo("0000000111"));
        }
    }
}