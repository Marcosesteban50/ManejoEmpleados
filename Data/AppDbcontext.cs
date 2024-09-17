using Microsoft.EntityFrameworkCore;
using ManejoClientes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using ManejoEmpleados.Models;

namespace ManejoClientes.Data
{
    public class AppDbcontext : IdentityDbContext
    {
        public AppDbcontext(DbContextOptions<AppDbcontext> options) : base(options)
        {
        }


        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Departamentos> Departamentos { get; set; }
        public DbSet<Posicion> Posiciones { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            //base.OnModelCreating(builder);

            builder.Entity<Empleado>(tb =>
            {


                tb.Property(c => c.Id);



                tb.Property(c => c.Nombre).HasMaxLength(50);

                tb.Property(c => c.Email).HasMaxLength(50);

                tb.Property(c => c.Telefono).HasMaxLength(50);

                tb.Property(c => c.Salario).HasDefaultValue(0)
                .HasColumnType("decimal(18, 2)");
                


                tb.HasOne(c => c.Usuario)
              .WithMany()
             .HasForeignKey(c => c.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();







            });

            builder.Entity<Posicion>(tb =>
            {


                tb.Property(c => c.Id);



                tb.Property(c => c.Nombre).HasMaxLength(50);


                tb.HasOne(c => c.Usuario)
     .WithMany()
     .HasForeignKey(c => c.UsuarioId)
     .OnDelete(DeleteBehavior.Restrict) // Evita la eliminación en cascada
     .IsRequired();


            });
            builder.Entity<Departamentos>(tb =>
            {


                tb.Property(c => c.Id);



                tb.Property(c => c.Nombre).HasMaxLength(50);


                tb.HasOne(c => c.Usuario)
               .WithMany()
                .HasForeignKey(c => c.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict) // Evita la eliminación en cascada
                .IsRequired();


            });





            builder.Entity<Usuario>(tb =>
            {

                tb.Property(c => c.Id).HasColumnType("nvarchar(450)");


                tb.Property(u => u.Email).HasMaxLength(50);
                tb.Property(u => u.EmailNormalizado).HasMaxLength(50);
                tb.Property(u => u.PasswordHash).HasColumnType("nvarchar(max)");
                tb.Property(u => u.AspNetUserId).HasColumnType("nvarchar(max)");
                tb.Property(u => u.EmpleadosContratados).HasDefaultValue(0);
                tb.Property(u => u.EmpleadosDespedidos).HasDefaultValue(0);
                tb.Property(u => u.SalarioEmpleados).HasDefaultValue(0)
                .HasColumnType("decimal(18, 2)");
                





            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });

            builder.Entity<IdentityRole<string>>(entity =>
            {
                entity.HasKey(r => r.Id);
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.HasKey(r => new { r.UserId, r.RoleId });
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });







            builder.Entity<Empleado>().ToTable("Empleados");
            builder.Entity<Posicion>().ToTable("Posiciones");
            builder.Entity<Departamentos>().ToTable("Departamentos");
            builder.Entity<Usuario>().ToTable("Usuarios");
        }
    }
}
