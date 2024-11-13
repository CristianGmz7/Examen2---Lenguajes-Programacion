using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Databases.LogDatabase.Entities;

public class LogEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("action")]
    public string Action { get; set; }      //GET, POST, PUT, DELETE

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}
