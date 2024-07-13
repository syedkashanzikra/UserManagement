using LoginwithEmail.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginwithEmail.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }
    }
}
