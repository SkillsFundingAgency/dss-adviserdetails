using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.AdviserDetail.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource<T>(T resource);
    }
}