using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.AdviserDetail.Models;
using NCS.DSS.AdviserDetails.Models;

namespace NCS.DSS.AdviserDetail.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAdviserDetail resource, bool validateModelForPost);
    }
}