using EnterpriseTaskManagement.Application.DTOs.Comment;
using EnterpriseTaskManagement.Application.Repositories;
using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Services.CommentService
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, int userId)
        {
            var comment = new Comment(
                request.Content,
                request.TaskId,
                userId
            );

            var createdComment = await _commentRepository.AddCommentAsync(comment);

            return new CommentDto
            {
                Id = createdComment.Id,
                Content = createdComment.Content,
                CreatedAt = createdComment.CreatedAt,
                TaskId = createdComment.TaskId,
                UserId = createdComment.UserId
            };
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByTaskIdAsync(int taskId)
        {
            var comments = await _commentRepository
                .GetByTaskIdAsync(taskId);

            return comments.Select(comment => new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TaskId = comment.TaskId,
                UserId = comment.UserId
            });
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);

            if (comment == null)
            {
                return false;
            }

            if (comment.UserId != userId)
            {
                return false;
            }

            await _commentRepository.DeleteAsync(comment);

            return true;
        }
    }
}