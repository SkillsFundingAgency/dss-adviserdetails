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
            var diversity = new Models.AdviserDetail();
            
            // Act
            diversity.SetDefaultValues();

            // Assert
            Assert.IsNotNull(diversity.LastModifiedDate);
        }

        [Test]
        public void AdviserDetailTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            // Arrange
            var diversity = new Models.AdviserDetail { LastModifiedDate = DateTime.MaxValue };

            // Act
            diversity.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, diversity.LastModifiedDate);
        }

        [Test]
        public void AdviserDetailTests_CheckAdviserDetailIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var diversity = new Models.AdviserDetail();

            // Act
            diversity.SetIds(It.IsAny<string>(), It.IsAny<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, diversity.AdviserDetailId);
        }

        [Test]
        public void AdviserDetailTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            // Arrange
            var diversity = new Models.AdviserDetail();

            // Act
            diversity.SetIds("0000000000", It.IsAny<string>());

            // Assert
            Assert.AreEqual("0000000000", diversity.LastModifiedTouchpointId);
        }
    }
}
