using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Dtos.Entries;

namespace ExamenU2LP.Services.Interfaces;

public interface IEntriesService
{
    Task<ResponseDto<EntryResponseDto>> CreateEntryAsync(EntryCreateDto dto);
    Task<ResponseDto<EntryResponseDto>> EditEntryAsync(EntryEditDto dto, int entryNumber);
    Task<ResponseDto<PaginationDto<List<EntryResponseDto>>>> GetEntriesListAsync(int page = 1);
    Task<ResponseDto<EntryResponseDto>> WriteOff(int entryNumber);
}
