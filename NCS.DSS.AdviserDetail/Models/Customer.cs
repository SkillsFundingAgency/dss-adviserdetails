using DFC.Swagger.Standard.Annotations;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NCS.DSS.AdviserDetail.Models
{
    public class Customer 
    {      
        [JsonProperty(PropertyName = "id")]
        public Guid? CustomerId { get; set; }      
        public DateTime? DateOfRegistration { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string UniqueLearnerNumber { get; set; }
        public bool? OptInUserResearch { get; set; }
        public bool? OptInMarketResearch { get; set; }
        public DateTime? DateOfTermination { get; set; }
        public string IntroducedByAdditionalInfo { get; set; }
        public string SubcontractorId { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedTouchpointId { get; set; }
    }
}
