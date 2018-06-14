using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.AdviserDetail.Models
{
    public class AdviserDetail
    {
        [Display(Description = "Unique identifier of the adviser involved in the interaction. ")]
        public Guid AdviserDetailId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Description = "Name of the adviser")]
        public string AdviserName { get; set; }

        [StringLength(100)]
        [Display(Description = "Email address of the adviser")]
        public string AdviserEmailAddress { get; set; }

        [StringLength(100)]
        [Display(Description = "Contact number of the adviser")]
        public string AdviserContactNumber { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}