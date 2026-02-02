using KeremProject1backend.Operations;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using KeremProject1backend.Models.DTOs.Requests;
using KeremProject1backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController : BaseController
{
    private readonly MessageOperations _messageOperations;

    public MessageController(MessageOperations messageOperations, SessionService sessionService) : base(sessionService)
    {
        _messageOperations = messageOperations;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
    {
        var result = await _messageOperations.StartConversationAsync(request.InstitutionId, request.ClassroomId, request.Title, request.IsGroup);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var result = await _messageOperations.SendMessageAsync(request.ConversationId, request.Content, request.Type, request.AttachedExamId, request.AttachedResultId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("history/{conversationId}")]
    public async Task<IActionResult> GetHistory(int conversationId)
    {
        var result = await _messageOperations.GetMessagesAsync(conversationId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("send-to-class")]
    public async Task<IActionResult> SendToClass([FromBody] SendToClassRequest request)
    {
        var result = await _messageOperations.SendToClassAsync(request.ClassroomId, request.ReportCardIds);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] bool forceRefresh = false)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.GetConversationsAsync(userId, page, limit, forceRefresh);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("conversation/{id}")]
    public async Task<IActionResult> GetConversation(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.GetConversationAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("conversation/{id}")]
    public async Task<IActionResult> UpdateConversation(int id, [FromBody] UpdateConversationRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.UpdateConversationAsync(id, request.Title, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("conversation/{id}")]
    public async Task<IActionResult> DeleteConversation(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.DeleteConversationAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("conversation/{id}/leave")]
    public async Task<IActionResult> LeaveConversation(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.LeaveConversationAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.DeleteMessageAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMessage(int id, [FromBody] UpdateMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.UpdateMessageAsync(id, request.Content, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("conversation/{id}/mark-read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.MarkReadAsync(id, userId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMessages(
        [FromQuery] string query,
        [FromQuery] int? conversationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _messageOperations.SearchMessagesAsync(query, conversationId, userId, page, limit);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

public class UpdateConversationRequest
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateMessageRequest
{
    public string Content { get; set; } = string.Empty;
}
