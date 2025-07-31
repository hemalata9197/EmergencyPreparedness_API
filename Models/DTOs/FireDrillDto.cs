using EmergencyManagement.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.DTOs
{
    public class FireDrillDto
    {     
        public Dictionary<string, object> FormData { get; set; } = new();
        public string EntryStatus { get; set; } = string.Empty;    
        public int SubmittedBy { get; set; }      
        public int UnitId { get; set; }
        public string Status { get; set; } = string.Empty;

    }
}
