using AutoMapper;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Dtos.EntriesDetails;

namespace ExamenU2LP.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        //MapsForEntryDetails();
    }

    private void MapsForEntryDetails()
    {
        CreateMap<EntryDetailEntity, EntryDetailResponseDto>();
        CreateMap<EntryDetailCreateDto, EntryDetailEntity>();
        //CreateMap<EntryDetailEditDto, EntryDetailEntity>();       //EntryDetailEditDto aun no existe
    }
}
