using Moq;
using NUnit.Framework;
using System;

namespace NCS.DSS.AdviserDetail.Tests.ModelTests
{
    [TestFixture]
    public class AdviserDetailTests
    {
        [Test]
        public void AdviserDetailTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var adviserDetail = new Models.AdviserDetail();

            // Act
            adviserDetail.SetDefaultValues();

            // Assert            
            Assert.That(adviserDetail.LastModifiedDate, Is.Not.Null);
        }

        [Test]
        public void AdviserDetailTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var adviserDetail = new Models.AdviserDetail { LastModifiedDate = DateTime.MaxValue };

            // Act
            adviserDetail.SetDefaultValues();

            // Assert            
            Assert.That(adviserDetail.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AdviserDetailTests_CheckAdviserDetailIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var adviserDetail = new Models.AdviserDetail();

            // Act
            adviserDetail.SetIds(It.IsAny<string>(), It.IsAny<string>());

            // Assert            
            Assert.That(adviserDetail.AdviserDetailId, Is.Not.SameAs(Guid.Empty));
        }

        [Test]
        public void AdviserDetailTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var adviserDetail = new Models.AdviserDetail();

            // Act
            adviserDetail.SetIds("0000000000", It.IsAny<string>());

            // Assert            
            Assert.That(adviserDetail.LastModifiedTouchpointId, Is.EqualTo("0000000000"));

        }
    }
}
