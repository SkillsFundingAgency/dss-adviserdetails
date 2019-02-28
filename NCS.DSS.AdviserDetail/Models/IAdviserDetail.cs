using System;

namespace NCS.DSS.AdviserDetails.Models
{
    public interface IAdviserDetail
    {
        string AdviserName { get; set; }
        string AdviserEmailAddress { get; set; }
        string AdviserContactNumber { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();
    }
}