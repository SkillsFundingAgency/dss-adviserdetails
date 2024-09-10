using NCS.DSS.AdviserDetails.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.AdviserDetail.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAdviserDetail resource, bool validateModelForPost);
    }
}