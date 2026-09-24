using FCG.Domain.Identidade.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Identidade.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        // Chave primária
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        // Nome
        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        // Email (Value Object)
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Valor)
                .HasColumnName("email")
                .HasMaxLength(256)
                .IsRequired();

            email.HasIndex(e => e.Valor)
                .IsUnique()
                .HasDatabaseName("ix_usuarios_email");
        });
    
        builder.Navigation(u => u.Email).IsRequired();

        // SenhaHash (Value Object)
        builder.OwnsOne(u => u.SenhaHash, senha =>
        {
            senha.Property(s => s.Senha)
                .HasColumnName("senha")
                .HasColumnType("char(69)")
                .IsRequired();
        });
    
        builder.Navigation(u => u.SenhaHash).IsRequired();

        // Perfil (enum)
        builder.Property(u => u.Perfil)
            .HasColumnName("perfil")
            .HasConversion<int>()
            .IsRequired();

        // Ativo
        builder.Property(u => u.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        // DataCriacao
        builder.Property(u => u.DataCriacao)
            .HasColumnName("data_criacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .ValueGeneratedNever();

        // DataAtualizacao
        builder.Property(u => u.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .HasColumnType("timestamp with time zone");
    }
}