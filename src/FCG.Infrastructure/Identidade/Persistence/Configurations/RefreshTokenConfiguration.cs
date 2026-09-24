using FCG.Domain.Identidade.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Infrastructure.Identidade.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        // Chave primária
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        // UsuarioId
        builder.Property(rt => rt.UsuarioId)
            .HasColumnName("usuario_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.HasIndex(rt => rt.UsuarioId)
            .HasDatabaseName("ix_refresh_tokens_usuario_id");

        builder.HasOne(rt => rt.Usuario)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // Token
        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");

        // CriadoEm
        builder.Property(rt => rt.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ExpiracaoEm
        builder.Property(rt => rt.ExpiracaoEm)
            .HasColumnName("expiracao_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Revogado
        builder.Property(rt => rt.Revogado)
            .HasColumnName("revogado")
            .HasDefaultValue(false)
            .IsRequired();

        // RevogadoEm
        builder.Property(rt => rt.RevogadoEm)
            .HasColumnName("revogado_em")
            .HasColumnType("timestamp with time zone");

        // MotivoRevogacao (enum)
        builder.Property(rt => rt.MotivoRevogacao)
            .HasColumnName("motivo_revogacao")
            .HasConversion<int?>();

        // SubstituidoPorId
        builder.Property(rt => rt.SubstituidoPorId)
            .HasColumnName("substituido_por_id")
            .HasColumnType("uuid");

        builder.HasOne(rt => rt.SubstituidoPor)
            .WithOne()
            .HasForeignKey<RefreshToken>(rt => rt.SubstituidoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}