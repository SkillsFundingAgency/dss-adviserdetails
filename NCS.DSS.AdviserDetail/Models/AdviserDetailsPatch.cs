using System;
using System.ComponentModel.DataAnnotations;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.AdviserDetails.Models;

namespace NCS.DSS.AdviserDetail.Models
{
    public class AdviserDetailPatch : IAdviserDetail
    {

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z]+(([\s'\,\.\-][a-zA-Z])?[a-zA-Z]*)*$")]
        [Display(Description = "Name of the adviser")]
        [Example(Description = "this is some text")]
        public string AdviserName { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")]
        [Display(Description = "Email address of the adviser")]
        [Example(Description = "example@sample.com")]
        public string AdviserEmailAddress { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*$")]
        [Display(Description = "Contact number of the adviser")]
        [Example(Description = "012345 678901")]
        public string AdviserContactNumber { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-22T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        [StringLength(50)]
        [Display(Description = "Identifier supplied by the touchpoint to indicate their subcontractor")]
        [Example(Description = "01234567899876543210")]
        public string SubcontractorId { get; set; }


        public void SetDefaultValues()
        {
            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }

        public void SetIds(string touchpointId, string subcontractorId)
        {
            LastModifiedTouchpointId = touchpointId;
            SubcontractorId = subcontractorId;
        }

    }
}