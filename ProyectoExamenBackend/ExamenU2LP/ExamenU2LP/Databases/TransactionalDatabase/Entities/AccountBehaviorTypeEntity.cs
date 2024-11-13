using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Databases.TransactionalDatabase.Entities;

[Table("account_behavior_types", Schema = "dbo")]
public class AccountBehaviorTypeEntity : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("type")]
    public string Type { get; set; }

    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
