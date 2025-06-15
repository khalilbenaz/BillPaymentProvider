using BillPaymentProvider.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BillPaymentProvider.Data
{
    /// <summary>
    /// Contexte de base de données principal pour l'application
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// Configurations des créanciers
        /// </summary>
        public DbSet<BillerConfiguration> BillerConfigurations { get; set; } = null!;

        /// <summary>
        /// Transactions (paiements et recharges)
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; } = null!;

        /// <summary>
        /// Logs des transactions
        /// </summary>
        public DbSet<TransactionLog> TransactionLogs { get; set; } = null!;

        /// <summary>
        /// Historique des paiements
        /// </summary>
        public DbSet<PaymentHistory> PaymentHistories { get; set; } = null!;

        /// <summary>
        /// Logs système
        /// </summary>
        public DbSet<LogEntry> LogEntries { get; set; } = null!;

        /// <summary>
        /// Utilisateurs de l'application
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des index et des contraintes

            // BillerConfiguration
            modelBuilder.Entity<BillerConfiguration>()
                .HasIndex(b => b.BillerCode)
                .IsUnique();

            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionId)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.SessionId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.CustomerReference);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.PhoneNumber);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Status);

            // TransactionLog
            modelBuilder.Entity<TransactionLog>()
                .HasIndex(t => t.TransactionId);

            // PaymentHistory
            modelBuilder.Entity<PaymentHistory>()
                .HasIndex(p => p.SessionId);

            modelBuilder.Entity<PaymentHistory>()
                .HasIndex(p => p.TransactionId);

            // LogEntry
            modelBuilder.Entity<LogEntry>()
                .HasIndex(l => l.Level);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
