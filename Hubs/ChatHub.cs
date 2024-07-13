using LoginwithEmail.Context;
using LoginwithEmail.Models;
using Microsoft.AspNetCore.SignalR;


namespace LoginwithEmail.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LoadPreviousMessages()
        {
            var messages = _context.Messages.OrderByDescending(m => m.Timestamp).Take(50).ToList();  // Adjust the number as needed
            foreach (var message in messages)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", message.UserName, message.Content);
            }
        }

        public async Task SendMessage(string userName, string content)
        {
            var message = new Message
            {
                UserName = userName,
                Content = content,
                Timestamp = System.DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", userName, content);
        }
    }
}
