using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamenU2LP.Databases.TransactionalDatabase.Entities;

[Table("chart_accounts", Schema = "dbo")]
public class ChartAccountEntity : BaseEntity
{
    //se debe crear logica para que siga un patron
    [Key]
    [Column("account_number")]
    public string AccountNumber { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("behavior_type_id")]
    public Guid BehaviorTypeId { get; set; }

    //llave foranea de BehaviorTypeId
    [ForeignKey(nameof(BehaviorTypeId))]
    public virtual AccountBehaviorTypeEntity BehaviorType { get; set; }

    [Column("allows_movement")]
    public bool AllowsMovement { get; set; }

    [Column("parent_account_number")]
    public string ParentAccountNumber { get; set; }

    //llave foranea de ParentAccountNumber
    [ForeignKey(nameof(ParentAccountNumber))]
    public virtual ChartAccountEntity ParentAccount { get; set; }

    [Column("is_disabled")]
    public bool IsDisabled { get; set; }

    //conexion de la relacion uno a muchos con la tabla intermedia
    public virtual IEnumerable<EntryDetailEntity> Details { get; set; }

    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
