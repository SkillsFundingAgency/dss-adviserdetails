using NCS.DSS.AdviserDetail.Validation;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.AdviserDetail.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenAdviserDetailIsNotSuppliedForPost()
        {
            // Arrange
            var AdviserDetail = new Models.AdviserDetail();
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(AdviserDetail, true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenNameIsNotSuppliedForPost()
        {
            // Arrange
            var AdviserDetail = new Models.AdviserDetail
            {
                AdviserContactNumber = "1111"
            };
            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(AdviserDetail, true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }
    }
}