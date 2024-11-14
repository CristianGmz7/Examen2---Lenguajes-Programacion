namespace ExamenU2LP.Dtos.EntriesDetails;

public class EntryDetailResponseDto
{
    public Guid Id { get; set; }

    //no se si iria el EntryNumber      //o si dentro de la logica al obtener la lista de esto filtrarlo por el id del EntryNumber
    public int EntryNumber { get; set; }

    public string AccountNumber { get; set; }

    public string EntryPosition { get; set; }

    public decimal Amount { get; set; }
}
