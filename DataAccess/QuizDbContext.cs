using Microsoft.EntityFrameworkCore;
using ModelsLibrary;

namespace DataAccess;

public class QuizDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<TopicModel> Topics { get; set; }
    public DbSet<QuestionModel> Questions { get; set; }
    public DbSet<AnswerModel> Answers { get; set; }
    public DbSet<DashboardModel> Dashboards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TopicModel>()
            .HasMany(t => t.Questions)
            .WithOne()
            .HasForeignKey(q => q.TopicModelId);

        modelBuilder.Entity<QuestionModel>()
            .HasMany(q => q.Answers)
            .WithOne()
            .HasForeignKey(a => a.QuestionnaireModelId);

        modelBuilder.Entity<DashboardModel>()
            .HasOne(d => d.Question)
            .WithMany()
            .HasForeignKey(d => d.QuestionId);
    }
}