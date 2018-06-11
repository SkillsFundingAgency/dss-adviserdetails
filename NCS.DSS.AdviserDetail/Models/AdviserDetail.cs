using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.AdviserDetail.Models
{
    public class AdviserDetail
    {
        public Guid AdviserDetailId { get; set; }

        [Required]
        [StringLength(100)]
        public string AdviserName { get; set; }

        [StringLength(100)]
        public string AdviserEmailAddress { get; set; }

        [StringLength(100)]
        public string AdviserContactNumber { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }

        public Guid LastModifiedTouchpointId { get; set; }
    }
}