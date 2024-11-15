using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Dtos.ChartsAccounts;
using ExamenU2LP.Dtos.Common;

namespace ExamenU2LP.Services.Interfaces
{
    public interface IChartsAccountsService
    {
        Task<ResponseDto<PaginationDto<List<ChartAccountResponseDto>>>> GetAllAccountsAsync(int page = 1);
        Task<ResponseDto<ChartAccountResponseDto>> GetAccountByIdAsync (string accountNumber);
        Task<ResponseDto<List<ChartAccountResponseDto>>> GetEnabledAccountsWithoutChildrenAsync();
        Task<ResponseDto<ChartAccountResponseDto>> CreateChartAccountAsync(ChartAccountCreateDto dto);
        Task<ResponseDto<ChartAccountResponseDto>> EditAccountAsync(ChartAccountEditDto dto, string accountNumber);
        Task<ResponseDto<ChartAccountResponseDto>> SwitchDisableAccountAsync(string accountNumber);
    }
}
