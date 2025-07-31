using EmergencyManagement.Models.DTOs.Task;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EmergencyManagement.Models.DTOs
{
    public class UpdateTaskStatusDto
    {
        public int fireDrillId { get; set; }
        public int taskId { get; set; }
        public int taskCreatedForId { get; set; }
        public int taskstatusId { get; set; }
        public string remark { get; set; }=string.Empty;
        public DateTime TargetDate { get; set; }
        public int modifiedBy { get; set; }

        [JsonPropertyName("uploadDocument")] // Only needed if using System.Text.Json
        public TaskEvidenceDto? TaskEvidence { get; set; }

    }
}
