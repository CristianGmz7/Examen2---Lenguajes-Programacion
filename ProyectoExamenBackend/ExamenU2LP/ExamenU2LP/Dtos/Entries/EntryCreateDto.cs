using ExamenU2LP.Dtos.EntriesDetails;
using System.ComponentModel.DataAnnotations;

namespace ExamenU2LP.Dtos.Entries;

public class EntryCreateDto
{
    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public DateTime Date { get; set; }

    [Display(Name = "Descripcion")]
    [Required(ErrorMessage = "La {0} es obligatoria")]
    public string Description { get; set; }

    //por default la prop public bool IsEditable { get; set; } sera true
    public List<EntryDetailCreateDto> DebitAccounts { get; set; }
    public List<EntryDetailCreateDto> CreditAccounts { get; set; }
}
