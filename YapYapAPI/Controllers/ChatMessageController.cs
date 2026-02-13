using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YapYapAPI.Data;
using YapYapAPI.Models;

namespace YapYapAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatMessageController : ControllerBase
    {
        private readonly YapYapDbContext _context;

        public ChatMessageController(YapYapDbContext context)
        {
            _context = context;
        }

        [HttpGet("chat/{chatId}")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatMessages(int chatId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var chat = await _context.Chats.FindAsync(chatId);

            if (chat == null)
            {
                return NotFound(new { message = "Chat not found" });
            }

            if (chat.UserOne != userId && chat.UserTwo != userId)
            {
                return Forbid();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Sender)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Message = m.Message,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name,
                    ChatId = m.ChatId,
                    GroupId = m.GroupId,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetGroupMessages(int groupId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var isMember = await _context.UserGroups
    .AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId);

            if (!isMember)
            {
                return Forbid();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.Sender)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Message = m.Message,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Name,
                    ChatId = m.ChatId,
                    GroupId = m.GroupId,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] CreateChatMessageDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (dto.ChatId.HasValue && dto.GroupId.HasValue)
            {
                return BadRequest(new { message = "Message can only be sent to either a chat or a group, not both" });
            }

            if (!dto.ChatId.HasValue && !dto.GroupId.HasValue)
            {
                return BadRequest(new { message = "Either chatId or groupId must be provided" });
            }

            if (dto.ChatId.HasValue)
            {
                var chat = await _context.Chats.FindAsync(dto.ChatId.Value);
                if (chat == null)
                {
                    return NotFound(new { message = "Chat not found" });
                }

                if (chat.UserOne != userId && chat.UserTwo != userId)
                {
                    return Forbid();
                }
            }

            if (dto.GroupId.HasValue)
            {
                var isMember = await _context.UserGroups
                    .AnyAsync(ug => ug.GroupId == dto.GroupId.Value && ug.UserId == userId);

                if (!isMember)
                {
                    return Forbid();
                }
            }

            var message = new ChatMessage
            {
                Message = dto.Message,
                SenderId = userId,
                ChatId = dto.ChatId,
                GroupId = dto.GroupId
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(userId);

            var messageDto = new ChatMessageDto
            {
                Id = message.Id,
                Message = message.Message,
                SenderId = message.SenderId,
                SenderName = sender?.Name ?? "",
                ChatId = message.ChatId,
                GroupId = message.GroupId,
                CreatedAt = message.CreatedAt
            };

            return Ok(messageDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] CreateChatMessageDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var message = await _context.ChatMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound(new { message = "Message not found" });
            }

            if (message.SenderId != userId)
            {
                return Forbid();
            }

            message.Message = dto.Message;
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var message = await _context.ChatMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound(new { message = "Message not found" });
            }

            if (message.SenderId != userId)
            {
                return Forbid();
            }

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}