using API.Models;
using API.Models.Context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Utils
{
    public class CampainHub : Hub
    {
        private readonly DBContext _context;

        public CampainHub(DBContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public async Task CampainUpdate(string id, string message)
        {
            await Clients.Group($"campain-{id}").SendAsync("CampainUpdate", message);
        }

        public async Task ListenCampain(string id)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"campain-{id}");
        }
    }
}
