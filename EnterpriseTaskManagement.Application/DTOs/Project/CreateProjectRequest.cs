using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs
{
    public class CreateProjectRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}