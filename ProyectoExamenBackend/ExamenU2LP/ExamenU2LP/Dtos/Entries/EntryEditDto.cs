using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Dtos.Entries;

public class EntryEditDto
{
    [Display(Name = "Descripcion")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public string Description { get; set; }
}
