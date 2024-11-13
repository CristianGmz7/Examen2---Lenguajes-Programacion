using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Databases.TransactionalDatabase.Entities;

[Table("entries", Schema = "dbo")]
public class EntryEntity : BaseEntity
{
    //en el EntryConfiguration se define que sea autoincrementable cuando se crea
    [Key]
    [Column("entry_number")]
    public int EntryNumber { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("description")]
    public string Description { get; set; }

    //conexion de la relacion uno a muchos con la tabla intermedia
    public virtual IEnumerable<EntryDetailEntity> Details { get; set; }

    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
