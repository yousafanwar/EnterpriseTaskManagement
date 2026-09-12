using EnterpriseTaskManagement.Application.DTOs.Comment;

namespace EnterpriseTaskManagement.Application.Services.CommentService
{
    public interface ICommentService
    {
        Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, int userId);

        Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId);

        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }
}