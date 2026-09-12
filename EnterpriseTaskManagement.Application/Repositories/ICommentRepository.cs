using EnterpriseTaskManagement.Domain.Entities;

namespace EnterpriseTaskManagement.Application.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> AddCommentAsync(Comment comment);

        Task<IEnumerable<Comment>> GetByTaskIdAsync(int taskId);

        Task<Comment?> GetByIdAsync(int id);

        Task DeleteAsync(Comment comment);
    }
}