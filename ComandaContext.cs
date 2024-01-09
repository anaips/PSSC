using Microsoft.EntityFrameworkCore;
using ProiectPSSC.Data.Models;

namespace ProiectPSSC.Data
{
    public class ComandaContext: DbContext
    {
        public ComandaContext(DbContextOptions<ComandaContext> options) : base(options)
        {
        }

        public DbSet<UtilizatorDto> Users { get; set; }
        public DbSet<ComandaDto> Commands { get; set; }
        //public DbSet<DetaliiComanda> Details { get; set; }
        public DbSet<ProdusDto> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UtilizatorDto>().ToTable("Utilizator").HasKey(s => s.ID_Utilizator);
            modelBuilder.Entity<ProdusDto>().ToTable("Produs").HasKey(s => s.ID_Produs);
            modelBuilder.Entity<ComandaDto>().ToTable("Comanda").HasKey(s => s.ID_Comanda);
           // modelBuilder.Entity<DetaliiComanda>().ToTable("DetaliiComanda").HasKey(s => s.ID_DetaliiComanda);
        }
    }
}