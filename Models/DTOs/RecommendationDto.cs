using EmergencyManagement.Models.Entities.Task;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmergencyManagement.Models.DTOs
{
    public class RecommendationDto
    {
        public int FireDrillId { get; set; }    
        public int ResponsibleUserId { get; set; } 
        public string RecommendationText { get; set; } = string.Empty;     
        public DateTime TargetDate { get; set; }    
        public int ActionStatusId { get; set; } 
        public int CreatedBy { get; set; }
        public int SeverityId {  get; set; }

        public List<Tasks> Tasks { get; set; } = new();

    }
}
