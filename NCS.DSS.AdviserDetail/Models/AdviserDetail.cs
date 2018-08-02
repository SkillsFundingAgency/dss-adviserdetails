using System;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.AdviserDetail.Annotations;

namespace NCS.DSS.AdviserDetail.Models
{
    public class AdviserDetail : IAdviserDetail
    {
        [Display(Description = "Unique identifier of the adviser involved in the interaction.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? AdviserDetailId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z]+(([\s'\,\.\-][a-zA-Z])?[a-zA-Z]*)*$")]
        [Display(Description = "Name of the adviser")]
        [Example(Description = "this is some text")]
        public string AdviserName { get; set; }

        [StringLength(100)]
        [Display(Description = "Email address of the adviser")]
        [Example(Description = "example@sample.com")]
        public string AdviserEmailAddress { get; set; }

        [StringLength(100)]
        [Display(Description = "Contact number of the adviser")]
        [Example(Description = "012345 678901")]
        public string AdviserContactNumber { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-22T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "d1307d77-af23-4cb4-b600-a60e04f8c3df")]
        public Guid? LastModifiedTouchpointId { get; set; }

        public void SetDefaultValues()
        {
            var adviserDetailId = Guid.NewGuid();
            AdviserDetailId = adviserDetailId;

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }

        public void Patch(AdviserDetailPatch adviserDetailPatch)
        {
            if (adviserDetailPatch == null)
                return;

            if(!string.IsNullOrEmpty(adviserDetailPatch.AdviserName))
                AdviserName = adviserDetailPatch.AdviserName;

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserEmailAddress))
                AdviserEmailAddress = adviserDetailPatch.AdviserEmailAddress;

            if (!string.IsNullOrEmpty(adviserDetailPatch.AdviserContactNumber))
                AdviserContactNumber = adviserDetailPatch.AdviserContactNumber;

            if (adviserDetailPatch.LastModifiedDate.HasValue)
                LastModifiedDate = adviserDetailPatch.LastModifiedDate;

            if (adviserDetailPatch.LastModifiedTouchpointId.HasValue)
                LastModifiedTouchpointId = adviserDetailPatch.LastModifiedTouchpointId;
        }
    }
}