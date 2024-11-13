using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Databases.TransactionalDatabase.Entities;

[Table("entry_details", Schema = "dbo")]
public class EntryDetailEntity : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("entry_number")]
    public int EntryNumber { get; set; }

    //llave foranea de EntryNumber
    [ForeignKey(nameof(EntryNumber))]
    public virtual EntryEntity Entry { get; set; }

    [Column("account_number")]
    public string AccountNumber { get; set; }

    //llave foranea de AccountNumber
    [ForeignKey(nameof(AccountNumber))]
    public virtual ChartAccountEntity Account { get; set; }

    [Column("entry_position")]
    public string EntryPosition { get; set; }

    //redondeo
    [Column("amount", TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
