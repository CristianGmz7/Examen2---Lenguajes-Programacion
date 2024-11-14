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

        //Peticiones GET

        [HttpPost]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        public async Task<ActionResult<ResponseDto<EntryResponseDto>>> Create (EntryCreateDto dto)
        {
            var response = await _entriesService.CreateEntryAsync(dto);

            return StatusCode(response.StatusCode, response);
        }

        //Peticiones PUT

    }
}
