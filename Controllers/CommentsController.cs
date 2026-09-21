using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CommentResponse>>> GetComments(int ticketId)
    {
        var result = await _commentService.GetCommentsAsync(ticketId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> AddComment(int ticketId, CreateCommentRequest request)
    {
        var result = await _commentService.AddCommentAsync(ticketId, request);
        return Created(string.Empty, result);
    }
}