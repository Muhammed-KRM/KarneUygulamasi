using KeremProject1backend.Operations;
using KeremProject1backend.Models.DTOs;
using KeremProject1backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeremProject1backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly MessageOperations _messageOperations;

    public MessageController(MessageOperations messageOperations)
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
}

public class StartConversationRequest
{
    public int InstitutionId { get; set; }
    public int? ClassroomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
}

public class SendMessageRequest
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public int? AttachedExamId { get; set; }
    public int? AttachedResultId { get; set; }
}

public class SendToClassRequest
{
    public int ClassroomId { get; set; }
    public List<int> ReportCardIds { get; set; } = new();
}
