using AppWebSistemaComandasDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppWebSistemaComandasDigital.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        public DbSet<Rol>           RolesComandas  { get; set; }
        public DbSet<Usuario>       Usuarios       { get; set; }
        public DbSet<Mesa>          Mesas          { get; set; }
        public DbSet<Categoria>     Categorias     { get; set; }
        public DbSet<Plato>         Platos         { get; set; }
        public DbSet<Pedido>        Pedidos        { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>(e => e.HasIndex(r => r.Nombre).IsUnique());

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.HasOne(u => u.Rol)
                 .WithMany(r => r.Usuarios)
                 .HasForeignKey(u => u.RolId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Mesa>(e =>
            {
                e.HasIndex(m => m.Numero).IsUnique();
                e.Property(m => m.Estado).HasConversion<string>();
            });

            modelBuilder.Entity<Categoria>(e => e.HasIndex(c => c.Nombre).IsUnique());

            modelBuilder.Entity<Plato>(e =>
                e.HasOne(p => p.Categoria)
                 .WithMany(c => c.Platos)
                 .HasForeignKey(p => p.CategoriaId)
                 .OnDelete(DeleteBehavior.Restrict));

            modelBuilder.Entity<Pedido>(e =>
            {
                e.Property(p => p.Estado).HasConversion<string>();
                e.HasOne(p => p.Mesa)
                 .WithMany(m => m.Pedidos)
                 .HasForeignKey(p => p.MesaId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Usuario)
                 .WithMany(u => u.Pedidos)
                 .HasForeignKey(p => p.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DetallePedido>(e =>
            {
                e.HasOne(d => d.Pedido)
                 .WithMany(p => p.Detalles)
                 .HasForeignKey(d => d.PedidoId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(d => d.Plato)
                 .WithMany(p => p.Detalles)
                 .HasForeignKey(d => d.PlatoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
