using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.ProviderEarnings.Domain.Entities;

namespace SFA.DAS.ProviderEarnings.DataAccess
{
    public class ProviderEarningsDataContext : DbContext
    {
        private readonly string _connectionString;

        public ProviderEarningsDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public virtual DbSet<Earning> Earnings { get; set; }
    }
}
