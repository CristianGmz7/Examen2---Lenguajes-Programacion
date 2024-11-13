using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamenU2LP.Databases.TransactionalDatabase.Entities;

[Table("account_balances", Schema = "dbo")]
public class AccountBalanceEntity : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Column("month")]
    public int Month { get; set; }

    [Column("account_number")]
    public string AccountNumber { get; set; }

    //llave foranea de AccountNumber
    [ForeignKey(nameof(AccountNumber))]
    public virtual ChartAccountEntity Account { get; set; }

    //investigar acerca del redondeo
    [Column("balance", TypeName = "decimal(18, 2)")]
    public decimal Balance { get; set; }

    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
