using ExamenU2LP.Dtos.EntriesDetails;

namespace ExamenU2LP.Dtos.Entries;

public class EntryResponseDto
{
    public int EntryNumber { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public bool IsEditable { get; set; }
    
    //si da problemas cambiarlo de List a IEnumerable
    public List<EntryDetailResponseDto> DebitAccounts { get; set; }
    public List<EntryDetailResponseDto> CreditAccounts { get; set; }
}
