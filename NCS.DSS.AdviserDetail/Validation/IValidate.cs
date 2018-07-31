using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.AdviserDetail.Models;

namespace NCS.DSS.AdviserDetail.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAdviserDetail resource, bool validateModelForPost);
    }
}