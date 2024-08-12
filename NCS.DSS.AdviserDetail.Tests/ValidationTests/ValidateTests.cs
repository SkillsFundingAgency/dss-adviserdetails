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
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);            
            Assert.That(result.Count, Is.EqualTo(2));
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
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}