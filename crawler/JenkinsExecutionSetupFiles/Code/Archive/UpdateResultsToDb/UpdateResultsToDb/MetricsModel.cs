namespace UpdateResultsToDb
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MetricsModel : DbContext
    {
        public MetricsModel()
            : base("name=MetricsModel")
        {
        }

        public virtual DbSet<RO_Results> RO_Results { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RO_Results>()
                .Property(e => e.Environment)
                .IsFixedLength();

            modelBuilder.Entity<RO_Results>()
                .Property(e => e.Browser)
                .IsFixedLength();

            modelBuilder.Entity<RO_Results>()
                .Property(e => e.Persona)
                .IsFixedLength();

            modelBuilder.Entity<RO_Results>()
                .Property(e => e.TestName)
                .IsFixedLength();

            modelBuilder.Entity<RO_Results>()
                .Property(e => e.TestCaseId)
                .IsFixedLength();

            modelBuilder.Entity<RO_Results>()
                .Property(e => e.Status)
                .IsFixedLength();
        }
    }
}
