using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using GeckoimagesApi.Models;

namespace GeckoimagesApi.Models
{
    public class GeckoContext : DbContext
    {
        public GeckoContext(DbContextOptions<GeckoContext> options) : base(options)
        {
        }

        public DbSet<Geckoimage> Geckoimages { get; set; } = null!;
    }
}