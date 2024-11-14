﻿using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Dtos.Entries;

namespace ExamenU2LP.Services.Interfaces;

public interface IEntriesService
{
    Task<ResponseDto<EntryResponseDto>> CreateEntryAsync(EntryCreateDto dto);
}