using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IAdviserDetail resource, bool validateModelForPost)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateAdviserDetailRules(resource, results, validateModelForPost);

            return results;
        }

        private void ValidateAdviserDetailRules(IAdviserDetail adviserDetailResource, List<ValidationResult> results, bool validateModelForPost)
        {
            if (adviserDetailResource == null)
                return;

            if (validateModelForPost)
            {
                if (string.IsNullOrWhiteSpace(adviserDetailResource.AdviserName))
                    results.Add(new ValidationResult("Adviser Name is a required field", new[] { "AdviserName" }));
            }

            if (adviserDetailResource.LastModifiedDate.HasValue && adviserDetailResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));

        }

    }
}
