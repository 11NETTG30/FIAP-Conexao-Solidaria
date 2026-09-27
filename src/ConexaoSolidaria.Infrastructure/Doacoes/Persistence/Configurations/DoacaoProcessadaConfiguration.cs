using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.Infrastructure.Doacoes.Persistence.Configurations;

public class DoacaoProcessadaConfiguration : IEntityTypeConfiguration<DoacaoProcessada>
{
    public void Configure(EntityTypeBuilder<DoacaoProcessada> builder)
    {
        builder.ToTable("doacoes_processadas");

        builder.HasKey(p => p.IdDoacao);

        builder.Property(p => p.IdDoacao)
            .HasColumnName("id_doacao")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(p => p.ProcessadaEm)
            .HasColumnName("processada_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
