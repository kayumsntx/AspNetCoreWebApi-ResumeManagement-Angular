using Microsoft.EntityFrameworkCore;

namespace ResumeApi.Models
{
    public class ServerContext:DbContext
    {
        public ServerContext(DbContextOptions<ServerContext> opp) : base(opp)
        { }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Experience> Experiences { get; set; }
        public virtual DbSet<ExperienceTitle> ExperienceTitles { get; set; }
    }
}
