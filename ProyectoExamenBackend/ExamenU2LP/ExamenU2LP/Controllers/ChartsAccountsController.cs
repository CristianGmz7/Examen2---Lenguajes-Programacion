using Azure;
using ExamenU2LP.Constants;
using ExamenU2LP.Dtos.ChartsAccounts;
using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamenU2LP.Controllers
{
    [Route("api/chartsAccounts")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChartsAccountsController : ControllerBase
    {
        private readonly IChartsAccountsService _chartsAccountsService;

        public ChartsAccountsController(IChartsAccountsService chartsAccountsService)
        {
            this._chartsAccountsService = chartsAccountsService;
        }

        // 1. Obtener todas las cuentas con paginación
        [HttpGet]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        public async Task<ActionResult<ResponseDto<PaginationDto<List<ChartAccountResponseDto>>>>> GetAllAccounts(int page = 1)
        {
            var response = await _chartsAccountsService.GetAllAccountsAsync(page);
            return StatusCode(response.StatusCode, response);
        }

        // 6. Obtener cuenta por ID
        [HttpGet("{accountNumber}")]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        //ya funciona
        public async Task<ActionResult<ResponseDto<ChartAccountResponseDto>>> GetAccountById(string accountNumber)
        {
            var response = await _chartsAccountsService.GetAccountByIdAsync(accountNumber);
            return StatusCode(response.StatusCode, response);
        }

        // 2. Obtener cuentas habilitadas sin cuentas hijas (AllowsMovement en true)
        [HttpGet("enabled-no-children")]
        //ya funciona
        public async Task<ActionResult<ResponseDto<List<ChartAccountResponseDto>>>> GetEnabledAccountsWithoutChildren()
        {
            var response = await _chartsAccountsService.GetEnabledAccountsWithoutChildrenAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        //ya funciona
        public async Task<ActionResult<ResponseDto<ChartAccountResponseDto>>> Create(ChartAccountCreateDto dto)
        {
            var response = await _chartsAccountsService.CreateChartAccountAsync(dto);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{accountNumber}")]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        public async Task<ActionResult<ResponseDto<ChartAccountResponseDto>>> Edit (ChartAccountEditDto dto, string accountNumber)
        {
            var response = await _chartsAccountsService.EditAccountAsync(dto, accountNumber);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("switchDisabledAccount/{accountNumber}")]
        [Authorize(Roles = $"{RolesConstant.USER}")]
        public async Task<ActionResult<ResponseDto<ChartAccountResponseDto>>> SwitchDisabledAccount(string accountNumber)
        {
            var response = await _chartsAccountsService.SwitchDisableAccountAsync(accountNumber);
            return StatusCode(response.StatusCode, response);
        }
    }
}
