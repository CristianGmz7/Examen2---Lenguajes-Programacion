using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExamenU2LP.Databases.TransactionalDatabase.Configuration;

public class EntryConfiguration : IEntityTypeConfiguration<EntryEntity>
{
    public void Configure(EntityTypeBuilder<EntryEntity> builder)
    {
        //cuando se este creando
        builder.HasOne((e) => e.CreatedByUser)      //propiedad virtual
            .WithMany()
            .HasForeignKey((e) => e.CreatedBy)       //CreatedBy es la llave foranea
            .HasPrincipalKey((e) => e.Id);           //esto representa la tabla de usuarios
                                                     //.IsRequired();

        //cuando se este editando
        builder.HasOne((e) => e.UpdatedByUser)      //propiedad virtual
            .WithMany()
            .HasForeignKey((e) => e.UpdatedBy)       //CreatedBy es la llave foranea
            .HasPrincipalKey((e) => e.Id);           //esto representa la tabla de usuarios
                                                     //.IsRequired();

        //hacer que la partida sea autoincrementable cuando se inserte una nueva
        builder.Property(p => p.EntryNumber)
            .ValueGeneratedOnAdd();
    }
}
