using ConexaoSolidaria.Domain.Doacoes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Configurations;

public class DoacaoConfiguration : IEntityTypeConfiguration<Doacao>
{
    public void Configure(EntityTypeBuilder<Doacao> builder)
    {
        builder.ToTable("doacoes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(d => d.IdCampanha)
            .HasColumnName("id_campanha")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(d => d.IdCampanha)
            .HasDatabaseName("ix_doacoes_id_campanha");

        builder.Property(d => d.IdDoador)
            .HasColumnName("id_doador")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(d => d.IdDoador)
            .HasDatabaseName("ix_doacoes_id_doador");

        builder.Property(d => d.ValorDoacao)
            .HasColumnName("valor_doacao")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.DataCriacao)
            .HasColumnName("data_criacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(d => d.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .HasColumnType("timestamp with time zone");
    }
}
