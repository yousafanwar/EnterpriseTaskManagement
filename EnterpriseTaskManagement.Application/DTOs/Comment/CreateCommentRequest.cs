using System.ComponentModel.DataAnnotations;

namespace EnterpriseTaskManagement.Application.DTOs.Comment
{
    public class CreateCommentRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        [Required]
        public int TaskId { get; set; }
    }
}