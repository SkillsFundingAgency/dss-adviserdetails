using System;

namespace NCS.DSS.AdviserDetail.Models
{
    public interface IAdviserDetail
    {
        string AdviserName { get; set; }
        string AdviserEmailAddress { get; set; }
        string AdviserContactNumber { get; set; }
        DateTime? LastModifiedDate { get; set; }
        Guid? LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();

    }
}