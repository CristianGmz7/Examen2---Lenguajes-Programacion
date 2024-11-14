using ExamenU2LP.Constants;
using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Dtos.Entries;
using ExamenU2LP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExamenU2LP.Controllers
{
    [Route("api/entries")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EntriesController : ControllerBase
    {
        private readonly IEntriesService _entriesService;

        public EntriesController(IEntriesService entriesService)
        {
            this._entriesService = entriesService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<PaginationDto<List<EntryResponseDto>>>>> GetAll(
                int page = 1
            )
        {
            var response = await _entriesService.GetEntriesListAsync(page);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{entryNumber}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<EntryResponseDto>>> GetById (int entryNumber)
        {
            var response = await _entriesService.GetEntryByIdAsync(entryNumber);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        public async Task<ActionResult<ResponseDto<EntryResponseDto>>> Create (EntryCreateDto dto)
        {
            var response = await _entriesService.CreateEntryAsync(dto);

            return StatusCode(response.StatusCode, response);
        }

        //Peticiones PUT
        [HttpPut("{entryNumber}")]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        //ojo con el tipo de dato que se recibe de id si da error o no funciona cambiarlo a string y hacer parseo en el metodo
        public async Task<ActionResult<ResponseDto<EntryResponseDto>>> Edit (EntryEditDto dto, int entryNumber)
        {
            var response = await _entriesService.EditEntryAsync(dto, entryNumber);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("writeOff/{entryNumber}")]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        public async Task <ActionResult<ResponseDto<EntryResponseDto>>> WriteOff (int entryNumber)
        {
            var response = await _entriesService.WriteOff(entryNumber);
            return StatusCode(response.StatusCode, response);
        }
    }
}
