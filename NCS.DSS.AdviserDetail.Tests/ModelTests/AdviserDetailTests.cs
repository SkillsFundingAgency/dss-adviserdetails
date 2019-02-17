using System;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.AdviserDetail.Tests.ModelTests
{

    [TestFixture]
    public class AdviserDetailTests
    {

        [Test]
        public void AdviserDetailTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.AdviserDetail();
            diversity.SetDefaultValues();

            // Assert
            Assert.IsNotNull(diversity.LastModifiedDate);
        }

        [Test]
        public void AdviserDetailTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var diversity = new Models.AdviserDetail { LastModifiedDate = DateTime.MaxValue };

            diversity.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, diversity.LastModifiedDate);
        }

        [Test]
        public void AdviserDetailTests_CheckAdviserDetailIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.AdviserDetail();

            diversity.SetIds(Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, diversity.AdviserDetailId);
        }

       
        [Test]
        public void AdviserDetailTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var diversity = new Models.AdviserDetail();

            diversity.SetIds("0000000000");

            // Assert
            Assert.AreEqual("0000000000", diversity.LastModifiedTouchpointId);
        }

        

    }
}
