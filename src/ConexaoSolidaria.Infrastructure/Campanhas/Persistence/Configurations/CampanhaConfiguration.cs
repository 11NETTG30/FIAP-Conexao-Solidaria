using ConexaoSolidaria.Domain.Campanhas.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.Infrastructure.Campanhas.Persistence.Configurations;

public class CampanhaConfiguration : IEntityTypeConfiguration<Campanha>
{
    public void Configure(EntityTypeBuilder<Campanha> builder)
    {
        builder.ToTable("campanhas");

        // Chave primária
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        // Titulo
        builder.Property(c => c.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(150)
            .IsRequired();

        // Descricao
        builder.Property(c => c.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(2000)
            .IsRequired();

        // DataInicio
        builder.Property(c => c.DataInicio)
            .HasColumnName("data_inicio")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // DataFim
        builder.Property(c => c.DataFim)
            .HasColumnName("data_fim")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // MetaFinanceira
        builder.Property(c => c.MetaFinanceira)
            .HasColumnName("meta_financeira")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        // Status (enum) — gravado como texto para o worker de doações filtrar
        // com "status = 'Ativa'" direto no SQL
        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_campanhas_status");

        // ValorArrecadado — incrementado pelo worker de doações
        builder.Property(c => c.ValorArrecadado)
            .HasColumnName("valor_arrecadado")
            .HasColumnType("numeric(18,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        // DataCriacao
        builder.Property(c => c.DataCriacao)
            .HasColumnName("data_criacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .ValueGeneratedNever();

        // DataAtualizacao
        builder.Property(c => c.DataAtualizacao)
            .HasColumnName("data_atualizacao")
            .HasColumnType("timestamp with time zone");
    }
}
