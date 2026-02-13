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
    public class FriendController : ControllerBase
    {
        private readonly YapYapDbContext _context;

        public FriendController(YapYapDbContext context)
        {
            _context = context;
        }

                [HttpGet]
        public async Task<ActionResult<IEnumerable<FriendDto>>> GetMyFriends()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var friends = await _context.Friends
                .Where(f => f.UserOne == userId || f.UserTwo == userId)
                .Include(f => f.UserOneNavigation)
                .Include(f => f.UserTwoNavigation)
                .ToListAsync();

            var friendDtos = friends.Select(f =>
            {
                var friend = f.UserOne == userId ? f.UserTwoNavigation : f.UserOneNavigation;
                return new FriendDto
                {
                    Id = f.Id,
                    UserId = friend.Id,
                    UserName = friend.Name,
                    UserBio = friend.BIO,
                    UserStatusId = friend.status_id
                };
            }).ToList();

            return Ok(friendDtos);
        }

                [HttpGet("{userId}")]
        public async Task<ActionResult<FriendDto>> CheckFriendship(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var friendship = await _context.Friends
                .Where(f =>
                    (f.UserOne == currentUserId && f.UserTwo == userId) ||
                    (f.UserOne == userId && f.UserTwo == currentUserId))
                .Include(f => f.UserOneNavigation)
                .Include(f => f.UserTwoNavigation)
                .FirstOrDefaultAsync();

            if (friendship == null)
            {
                return NotFound(new { message = "Not friends" });
            }

            var friend = friendship.UserOne == currentUserId ? friendship.UserTwoNavigation : friendship.UserOneNavigation;

            var friendDto = new FriendDto
            {
                Id = friendship.Id,
                UserId = friend.Id,
                UserName = friend.Name,
                UserBio = friend.BIO,
                UserStatusId = friend.status_id
            };

            return Ok(friendDto);
        }

                [HttpDelete("{friendshipId}")]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var friendship = await _context.Friends.FindAsync(friendshipId);

            if (friendship == null)
            {
                return NotFound(new { message = "Friendship not found" });
            }

                        if (friendship.UserOne != userId && friendship.UserTwo != userId)
            {
                return Forbid();
            }

            _context.Friends.Remove(friendship);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}