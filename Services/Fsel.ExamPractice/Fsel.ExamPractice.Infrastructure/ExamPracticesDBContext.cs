namespace Fsel.ExamPractice.Infrastructure
{
    using Fsel.Core.Base;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Infrastructure.Configs;

    public class ExamPracticesReadDBContext : ExamPracticesBaseDBContext
    {
        public ExamPracticesReadDBContext(DbContextOptions<ExamPracticesReadDBContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    public class ExamPracticesDBContext : ExamPracticesBaseDBContext
    {
        public ExamPracticesDBContext(DbContextOptions<ExamPracticesDBContext> options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }
    }

    public class ExamPracticesBaseDBContext : BaseDbContext
    {
        public ExamPracticesBaseDBContext(DbContextOptions options, IMediator mediator, AuthContext authContext)
            : base(options, mediator, authContext)
        {
        }

        public DbSet<ExamPractice> ExamPractices { get; set; }
        public DbSet<ExamPracticeSection> ExamPracticeSections { get; set; }
        public DbSet<ExamPracticeAnswer> ExamPracticeAnswers { get; set; }
        public DbSet<ExamPracticeRetry> ExamPracticeRetrys { get; set; }
        public DbSet<ExamPracticeResult> ExamPracticeResults { get; set; }
        public DbSet<ExamPracticeAICriteriaSetting> ExamPracticeAICriteriaSettings { get; set; }
        public DbSet<ExamPracticeAISetting> ExamPracticeAISettings { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<ExamPracticeSectionResult> ExamPracticeSectionResults { get; set; }
        public DbSet<ExamPracticeScore> ExamPracticeScores { get; set; }
        public DbSet<ProsodyScore> ProsodyScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.ApplyConfiguration(new ExamPracticeAnswerEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeRetryEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeSectionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeSectionResultEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeAISettingEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeAiCriteriaSettingEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPracticeScoreEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProsodyScoreEntityTypeConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
