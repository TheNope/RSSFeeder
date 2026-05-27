using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public class DatabaseContext : DbContext, IDatabaseContext
    {
        public DbSet<Sample> Samples { get; set; }

        private readonly ILogger<DatabaseContext> _logger;

        public DatabaseContext(ILogger<DatabaseContext> logger, DbContextOptions<DatabaseContext> options) : base(options)
        {
            _logger = logger;
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Sample>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Name).IsUnique();
                entity.Property(x => x.IsDeleted);
            });
        }
    }
}
