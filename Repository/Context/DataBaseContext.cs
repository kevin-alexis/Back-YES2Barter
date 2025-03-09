using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Context
{
    public class DataBaseContext : IdentityDbContext<ApplicationUser>
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PropuestaIntercambio>()
                .HasOne(p => p.UsuarioOfertante)
                .WithMany()
                .HasForeignKey(p => p.IdUsuarioOfertante)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropuestaIntercambio>()
                .HasOne(p => p.UsuarioReceptor)
                .WithMany()
                .HasForeignKey(p => p.IdUsuarioReceptor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropuestaIntercambio>()
                .HasOne(p => p.ObjetoOfertado)
                .WithMany()
                .HasForeignKey(p => p.IdObjetoOfertado)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropuestaIntercambio>()
                .HasOne(p => p.ObjetoSolicitado)
                .WithMany()
                .HasForeignKey(p => p.IdObjetoSolicitado)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensaje>()
               .HasOne(p => p.UsuarioEmisor)
               .WithMany()
               .HasForeignKey(p => p.IdUsuarioEmisor)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensaje>()
                .HasOne(p => p.UsuarioReceptor)
                .WithMany()
                .HasForeignKey(p => p.IdUsuarioReceptor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensaje>()
                .HasOne(p => p.Chat)
                .WithMany()
                .HasForeignKey(p => p.IdChat)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
               .HasOne(p => p.Usuario1)
               .WithMany()
               .HasForeignKey(p => p.IdUsuario1)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(p => p.Usuario2)
                .WithMany()
                .HasForeignKey(p => p.IdUsuario2)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>();
        }


        public DbSet<Log> Logs { get; set; }

        // Entidades del Proyecto
        public DbSet<Objeto> Objetos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<PropuestaIntercambio> PropuestasIntercambios { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Mensaje> Mensajes { get; set; }
    }
}
