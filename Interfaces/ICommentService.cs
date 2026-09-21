using HelpDesk.Api.Models.DTOs.Comments;

namespace HelpDesk.Api.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponse>> GetCommentsAsync(int ticketId);

    Task<CommentResponse> AddCommentAsync(int ticketId, CreateCommentRequest request);
}