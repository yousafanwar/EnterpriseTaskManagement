using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs
{
    public class CreateSkillRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}