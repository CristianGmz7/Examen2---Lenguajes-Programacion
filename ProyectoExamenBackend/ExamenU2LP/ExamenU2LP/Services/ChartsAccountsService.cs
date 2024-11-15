using AutoMapper;
using ExamenU2LP.Constants;
using ExamenU2LP.Databases.LogDatabase;
using ExamenU2LP.Databases.LogDatabase.Entities;
using ExamenU2LP.Databases.TransactionalDatabase;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Dtos.ChartsAccounts;
using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExamenU2LP.Services
{
    public class ChartsAccountsService : IChartsAccountsService
    {
        private readonly TransactionalContext _transactionalContext;
        private readonly LogContext _logContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ChartsAccountsService> _logger;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly int PAGE_SIZE;

        public ChartsAccountsService(
            TransactionalContext transactionalContext,
            LogContext logContext,
            IMapper mapper,
            ILogger<ChartsAccountsService> logger,
            IAuditService auditService,
            IConfiguration configuration
            ) 
        {
            this._transactionalContext = transactionalContext;
            this._logContext = logContext;
            this._mapper = mapper;
            this._logger = logger;
            this._auditService = auditService;
            this._configuration = configuration;
            PAGE_SIZE = configuration.GetValue<int>("Pagination:ChartAccountSize");
        }

        // Obtener todas las cuentas
        //funciona
        public async Task<ResponseDto<PaginationDto<List<ChartAccountResponseDto>>>> GetAllAccountsAsync(int page = 1)
        {
            int startIndex = (page - 1) * PAGE_SIZE;

            // Consulta principal para obtener todas las cuentas con sus tipos de comportamiento y cuentas padre
            var accountsQuery = _transactionalContext.ChartAccounts
                .Include(a => a.BehaviorType)
                .Include(a => a.ParentAccount);

            int totalAccounts = await accountsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalAccounts / PAGE_SIZE);

            // Obtener cuentas con paginación
            var accountsEntity = await accountsQuery
                .OrderBy(a => a.AccountNumber)
                .Skip(startIndex)
                .Take(PAGE_SIZE)
                .ToListAsync();

            // Obtener números de cuenta para buscar balances
            var accountNumbers = accountsEntity.Select(a => a.AccountNumber).ToList();
            var accountBalances = await _transactionalContext.AccountBalances
                .Where(ab => accountNumbers.Contains(ab.AccountNumber))
                .ToListAsync();

            await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_SEARCH_SUCCESS}");
            // Convertir entidades en DTOs
            var accountDtos = accountsEntity.Select(account =>
            {
                var balance = accountBalances
                    .FirstOrDefault(ab => ab.AccountNumber == account.AccountNumber)?.Balance ?? 0;

                return new ChartAccountResponseDto
                {
                    AccountNumber = account.AccountNumber,
                    Name = account.Name,
                    BehaviorTypeId = account.BehaviorTypeId,
                    BehaviorTypeName = account.BehaviorType.Type,
                    AllowsMovement = account.AllowsMovement,
                    ParentAccountNumber = account.ParentAccountNumber,
                    ParentAccountName = account.ParentAccount?.Name,
                    IsDisabled = account.IsDisabled,
                    Balance = balance
                };
            }).ToList();

            return new ResponseDto<PaginationDto<List<ChartAccountResponseDto>>>
            {
                StatusCode = 200,
                Status = true,
                Message = "Cuentas obtenidas satisfactoriamente",
                Data = new PaginationDto<List<ChartAccountResponseDto>>
                {
                    CurrentPage = page,
                    PageSize = PAGE_SIZE,
                    TotalItems = totalAccounts,
                    TotalPages = totalPages,
                    Items = accountDtos,
                    HasPreviousPage = page > 1,
                    HasNextPage = page < totalPages,
                }
            };
        }

        //obtener una cuenta por su numero de cuenta
        public async Task<ResponseDto<ChartAccountResponseDto>> GetAccountByIdAsync(string accountNumber)
        {
            try
            {
                // Buscar la cuenta por número de cuenta con sus relaciones
                var accountEntity = await _transactionalContext.ChartAccounts
                    .Include(a => a.BehaviorType)
                    .Include(a => a.ParentAccount)
                    .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

                // Verificar si la cuenta existe
                if (accountEntity == null)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_SEARCH_ERROR}");
                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        Status = false,
                        StatusCode = 404,
                        Message = "La cuenta no existe"
                    };
                }

                // Verificar si la cuenta está deshabilitada
                if (accountEntity.IsDisabled)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_SEARCH_ERROR}");
                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "No se puede obtener la cuenta, ya que está deshabilitada"
                    };
                }

                // Obtener el balance de la cuenta
                var accountBalanceEntity = await _transactionalContext.AccountBalances
                    .FirstOrDefaultAsync(ab => ab.AccountNumber == accountNumber);

                // Validar si existe un balance para la cuenta
                if (accountBalanceEntity == null)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_SEARCH_ERROR}");
                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "No existe el saldo de la cuenta"
                    };
                }

                // Crear el DTO de respuesta
                var responseDto = new ChartAccountResponseDto
                {
                    AccountNumber = accountEntity.AccountNumber,
                    Name = accountEntity.Name,
                    BehaviorTypeId = accountEntity.BehaviorTypeId,
                    BehaviorTypeName = accountEntity.BehaviorType?.Type, // Manejo de null
                    AllowsMovement = accountEntity.AllowsMovement,
                    ParentAccountNumber = accountEntity.ParentAccountNumber,
                    ParentAccountName = accountEntity.ParentAccount?.Name, // Manejo de null
                    IsDisabled = accountEntity.IsDisabled,
                    Balance = accountBalanceEntity.Balance
                };

                await LogActionAsync($"{MessagesLogsConstant.ENTRY_SEARCH_SUCCESS}");
                // Retornar la respuesta con los datos de la cuenta
                return new ResponseDto<ChartAccountResponseDto>
                {
                    Status = true,
                    StatusCode = 200,
                    Message = "Cuenta obtenida satisfactoriamente",
                    Data = responseDto
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al obtener la cuenta contable");

                // En caso de error
                return new ResponseDto<ChartAccountResponseDto>
                {
                    Status = false,
                    StatusCode = 500,
                    Message = "Se produjo un error al obtener la cuenta contable"
                };
            }
        }

        // obtener cuentas habilitadas
        public async Task<ResponseDto<List<ChartAccountResponseDto>>> GetEnabledAccountsWithoutChildrenAsync()
        {
            // Obtener cuentas habilitadas (AllowsMovement es true y IsDisabled es false)
            var enabledAccounts = await _transactionalContext.ChartAccounts
                .Include(a => a.BehaviorType)
                .Include(a => a.ParentAccount)
                .Where(a => a.AllowsMovement && !a.IsDisabled)          //aqui se filtran las cuentas que no estan deshabilitadas
                .ToListAsync();

            // Obtener números de cuenta que están referenciados como ParentAccountNumber en otras cuentas
            var parentAccountNumbers = await _transactionalContext.ChartAccounts
                .Where(c => c.ParentAccountNumber != null)
                .Select(c => c.ParentAccountNumber)
                .Distinct()
                .ToListAsync();

            // Filtrar solo las cuentas habilitadas que no están referenciadas en otras cuentas
            var accountsWithoutChildren = enabledAccounts
                .Where(a => !parentAccountNumbers.Contains(a.AccountNumber))
                .ToList();

            var accountNumbers = accountsWithoutChildren.Select(a => a.AccountNumber).ToList();

            var accountBalances = await _transactionalContext.AccountBalances
             .Where(ab => accountNumbers.Contains(ab.AccountNumber))
             .ToListAsync();

            // Crear la lista de DTOs
            var accountDtos = accountsWithoutChildren.Select(accountEntity =>
            {
                var balance = accountBalances
                    .FirstOrDefault(ab => ab.AccountNumber == accountEntity.AccountNumber)?.Balance ?? 0;

                return new ChartAccountResponseDto
                {
                    AccountNumber = accountEntity.AccountNumber,
                    Name = accountEntity.Name,
                    BehaviorTypeId = accountEntity.BehaviorTypeId,
                    BehaviorTypeName = accountEntity.BehaviorType.Type,
                    AllowsMovement = accountEntity.AllowsMovement,
                    ParentAccountNumber = accountEntity.ParentAccountNumber,
                    ParentAccountName = accountEntity.ParentAccount?.Name,
                    IsDisabled = accountEntity.IsDisabled,
                    Balance = balance,
                };
            }).ToList();

            await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_SEARCH_SUCCESS}");
            return new ResponseDto<List<ChartAccountResponseDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Cuentas habilitadas sin hijos obtenidas satisfactoriamente",
                Data = accountDtos
            };
        }

        //comprobado
        public async Task<ResponseDto<ChartAccountResponseDto>> CreateChartAccountAsync(ChartAccountCreateDto dto)
        {
            using (var transaction = await _transactionalContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validar que el número de cuenta padre exista
                    var parentAccount = await _transactionalContext.ChartAccounts
                                                                   .FirstOrDefaultAsync(x => x.AccountNumber == dto.ParentAccountNumber);
                    if (parentAccount == null)
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_ADD_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            Message = "El número de cuenta padre no existe.",
                            StatusCode = 400
                        };
                    }

                    // Validar que el nombre de la cuenta hija sea único dentro de la clase padre
                    bool isAccountNameUnique = await IsAccountNameUniqueAsync(dto.ParentAccountNumber, dto.Name);
                    if (!isAccountNameUnique)
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_ADD_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            Message = "El nombre de la cuenta hija no es único dentro de la clase padre.",
                            StatusCode = 400
                        };
                    }

                    // Validar y derivar el número de cuenta
                    var accountNumber = await DeriveAccountNumberAsync(dto.ParentAccountNumber);
                    if (string.IsNullOrEmpty(accountNumber))
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_ADD_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            Message = "No se pudo derivar el número de cuenta.",
                            StatusCode = 400
                        };
                    }

                    var parentAccountBehaviorId = parentAccount.BehaviorTypeId;

                    // Heredar el ID del tipo de comportamiento para la nueva cuneta
                    var accountBehavior = await _transactionalContext.AccountBehaviorTypes
                                                                     .FirstOrDefaultAsync(x => x.Id == parentAccountBehaviorId);
                    if (accountBehavior == null || accountBehavior.Id == Guid.Parse("33383121-F8A4-4AC7-9F8C-D25AD39ABD0F"))
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_ADD_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            Message = "El ID del tipo de comportamiento no es válido.",
                            StatusCode = 400
                        };
                    }

                    // 5. Crear la nueva cuenta
                    var newAccount = new ChartAccountEntity
                    {
                        AccountNumber = accountNumber,
                        Name = dto.Name,
                        BehaviorTypeId = parentAccountBehaviorId,
                        AllowsMovement = true,  // Configurado por defecto
                        IsDisabled = false,    // Configurado por defecto
                        ParentAccountNumber = dto.ParentAccountNumber
                    };

                    //adicionar a la tabla de cuentas para poder hacer el registro en la tabla de saldos
                    _transactionalContext.ChartAccounts.Add(newAccount); 
                    await _transactionalContext.SaveChangesAsync();

                    var parentAccountBalance = await _transactionalContext.AccountBalances
                        .FirstOrDefaultAsync(x => x.AccountNumber == parentAccount.AccountNumber);

                    // Transferir saldo si la cuenta padre tiene saldo y no tiene cuentas hijas
                    // y si tiene cuentas hijas solo crear la cuenta con saldo 0

                    var childAccountsCount = _transactionalContext.ChartAccounts
                        .Local // Trabaja con las entidades en memoria
                        .Count(x => x.ParentAccountNumber == parentAccount.AccountNumber);

                    // para que entre al if verifica si la cuenta del padre existe, que tenga saldo mayor que cero
                    // el ultimo verfica que la cuenta padre no sea cuenta padre de otras cuentas
                    if (parentAccountBalance != null && parentAccountBalance.Balance > 0 && childAccountsCount <= 1)
                    {
                        // Crear la nueva entrada de saldo para la nueva cuenta
                        var newAccountBalance = new AccountBalanceEntity
                        {
                            AccountNumber = newAccount.AccountNumber, // El número de cuenta de la nueva cuenta
                            Balance = parentAccountBalance.Balance,  // Transferir el saldo de la cuenta padre
                            Year = DateTime.Now.Year,  // Usa el año actual o el año correspondiente
                            Month = DateTime.Now.Month  // Usa el mes actual o el mes correspondiente
                        };

                        // Agregar el nuevo saldo a la tabla AccountBalances
                        _transactionalContext.AccountBalances.Add(newAccountBalance);

                        // Guardar los cambios en la base de datos
                        await _transactionalContext.SaveChangesAsync();
                    }
                    else
                    {
                        // Crear la nueva entrada de saldo para la nueva cuenta
                        var newAccountBalance = new AccountBalanceEntity
                        {
                            AccountNumber = newAccount.AccountNumber, // El número de cuenta de la nueva cuenta
                            Balance = 0,  // Transferir el saldo de la cuenta padre
                            Year = DateTime.Now.Year,  // Usa el año actual o el año correspondiente
                            Month = DateTime.Now.Month  // Usa el mes actual o el mes correspondiente
                        };

                        // Agregar el nuevo saldo a la tabla AccountBalances
                        _transactionalContext.AccountBalances.Add(newAccountBalance);

                        // Guardar los cambios en la base de datos
                        await _transactionalContext.SaveChangesAsync();
                    }

                    // Agregar la nueva cuenta              ESTO SE MOVIO HACIA MAS ARRIBA
                    //_transactionalContext.ChartAccounts.Add(newAccount);
                    //await _transactionalContext.SaveChangesAsync();

                    // Si la cuenta padre tiene AllowsMovement = true, actualizar a false
                    if (parentAccount.AllowsMovement)
                    {
                        parentAccount.AllowsMovement = false;
                    }

                    var accountBalanceEntity = await _transactionalContext.AccountBalances
                        .FirstOrDefaultAsync(ab => ab.AccountNumber == newAccount.AccountNumber);

                    //Posiblemente falte implementar verificacion anterior

                    // Confirmar la transacción
                    await _transactionalContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Crear DTO de respuesta
                    var responseDto = new ChartAccountResponseDto
                    {
                        AccountNumber = newAccount.AccountNumber,
                        Name = newAccount.Name,
                        BehaviorTypeId = newAccount.BehaviorTypeId,
                        BehaviorTypeName = accountBehavior.Type,
                        AllowsMovement = newAccount.AllowsMovement,
                        ParentAccountNumber = newAccount.ParentAccountNumber,
                        ParentAccountName = parentAccount.Name,
                        IsDisabled = newAccount.IsDisabled,
                        Balance =  accountBalanceEntity.Balance 
                    };

                    await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_ADD_SUCCESS}");
                    // Retornar respuesta exitosa
                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        Status = true,
                        Message = "Cuenta creada exitosamente.",
                        Data = responseDto,
                        StatusCode = 200
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear la cuenta en el servicio.");
                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        Status = false,
                        Message = $"Error interno: {ex.Message}",
                        StatusCode = 500
                    };
                }
            }
        }

        //comprobado
        public async Task<ResponseDto<ChartAccountResponseDto>> EditAccountAsync(ChartAccountEditDto dto, string accountNumber)
        {
            using (var transaction = await _transactionalContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var accountEntity = await _transactionalContext.ChartAccounts
                    .Include(a => a.BehaviorType) // Incluye BehaviorType
                    .Include(a => a.ParentAccount) // Incluye ParentAccount
                    .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

                    if (accountEntity == null)
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_UPDATE_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            StatusCode = 404,
                            Message = "No se encontro el registro"
                        };
                    }

                    // verificar que la cuenta no este deshabilitda
                    if(accountEntity.IsDisabled)
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_UPDATE_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            StatusCode = 404,
                            Message = "No se puede editar una partida que esta deshabilitada"
                        };
                    }

                    accountEntity.Name = dto.Name;

                    _transactionalContext.ChartAccounts.Update(accountEntity);

                    await _transactionalContext.SaveChangesAsync();

                    var accountBalanceEntity = await _transactionalContext.AccountBalances
                        .FirstOrDefaultAsync(ab => ab.AccountNumber == accountNumber);

                    if (accountBalanceEntity == null)
                    {
                        await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_UPDATE_ERROR}");
                        return new ResponseDto<ChartAccountResponseDto>
                        {
                            Status = false,
                            StatusCode = 400,
                            Message = "No existe el saldo de la cuenta"
                        };
                    }


                    //throw new Exception("Error para probar el rollback");
                    await transaction.CommitAsync();

                    var responseDto = new ChartAccountResponseDto
                    {
                        AccountNumber = accountEntity.AccountNumber,
                        Name = accountEntity.Name,
                        BehaviorTypeId = accountEntity.BehaviorTypeId,
                        BehaviorTypeName = accountEntity.BehaviorType.Type,
                        AllowsMovement = accountEntity.AllowsMovement,
                        ParentAccountNumber = accountEntity.ParentAccountNumber,
                        ParentAccountName = accountEntity.ParentAccount.Name,
                        IsDisabled = accountEntity.IsDisabled,
                        Balance = accountBalanceEntity.Balance,
                    };

                    await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_UPDATE_SUCCESS}");
                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        Status = true,
                        StatusCode = 200,
                        Message = "Registro editado satisfactoriamente",
                        Data = responseDto
                    };
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();

                    await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_UPDATE_ERROR}");
                    _logger.LogError(e, "Error al editar la cuenta contable");

                    return new ResponseDto<ChartAccountResponseDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = "Se produjo error al editar la cuenta contable"
                    };
                }
            }
        }

        // la comprobacion de si hay saldos pendientes se tiene que hacer hasta que se junte con el CRUD de partidas, solo probar habilitar y deshabilitar partidas sin saldo ni hijos
        public async Task<ResponseDto<ChartAccountResponseDto>> SwitchDisableAccountAsync(string accountNumber)
        {
            var accountEntity = await _transactionalContext.ChartAccounts
                .Include(a => a.BehaviorType)
                .Include(a => a.ParentAccount)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

            if (accountEntity == null)
            {
                await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_DISABLED_ERROR}");
                return new ResponseDto<ChartAccountResponseDto>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "No se encontró el registro"
                };
            }

            // Si la cuenta estaba deshabilitada, habilitarla de nuevo
            if (accountEntity.IsDisabled)
            {
                accountEntity.IsDisabled = false;

                var responseDto = new ChartAccountResponseDto
                {
                    AccountNumber = accountEntity.AccountNumber,
                    Name = accountEntity.Name,
                    BehaviorTypeId = accountEntity.BehaviorTypeId,
                    BehaviorTypeName = accountEntity.BehaviorType.Type,
                    AllowsMovement = accountEntity.AllowsMovement,
                    ParentAccountNumber = accountEntity.ParentAccountNumber,
                    ParentAccountName = accountEntity.ParentAccount?.Name,
                    IsDisabled = accountEntity.IsDisabled
                };

                _transactionalContext.ChartAccounts.Update(accountEntity);
                await _transactionalContext.SaveChangesAsync();

                await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_ENABLED_SUCCESS}");
                return new ResponseDto<ChartAccountResponseDto>
                {
                    Status = true,
                    StatusCode = 200,
                    Message = "Cuenta habilitada nuevamente",
                    Data = responseDto
                };
            }

            // Obtener todas las cuentas hijas de la cuenta actual
            var childAccounts = await _transactionalContext.ChartAccounts
                .Where(c => c.ParentAccountNumber == accountEntity.AccountNumber)
                .ToListAsync();

            // Verificar que todas las cuentas hijas estén deshabilitadas y tengan saldo cero
            var allChildrenDisabledAndZeroBalance = childAccounts.All(c => c.IsDisabled &&
                _transactionalContext.AccountBalances
                    .Where(b => b.AccountNumber == c.AccountNumber)
                    .Sum(b => b.Balance) == 0);

            // Verificar que el saldo de la cuenta actual es cero
            var currentAccountBalance = await _transactionalContext.AccountBalances
                .Where(b => b.AccountNumber == accountEntity.AccountNumber)
                .SumAsync(b => b.Balance);

            // Condiciones para deshabilitar la cuenta
            if ((childAccounts.Count == 0 && currentAccountBalance == 0) || allChildrenDisabledAndZeroBalance)
            {
                accountEntity.IsDisabled = true;

                _transactionalContext.ChartAccounts.Update(accountEntity);
                await _transactionalContext.SaveChangesAsync();

                await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_DISABLED_SUCCESS}");
                return new ResponseDto<ChartAccountResponseDto>
                {
                    Status = true,
                    StatusCode = 200,
                    Message = "Cuenta deshabilitada satisfactoriamente",
                    Data = new ChartAccountResponseDto
                    {
                        AccountNumber = accountEntity.AccountNumber,
                        Name = accountEntity.Name,
                        BehaviorTypeId = accountEntity.BehaviorTypeId,
                        AllowsMovement = accountEntity.AllowsMovement,
                        ParentAccountNumber = accountEntity.ParentAccountNumber,
                        ParentAccountName = accountEntity.ParentAccount?.Name,
                        IsDisabled = accountEntity.IsDisabled
                    }
                };
            }
            else
            {
                // Si la cuenta no puede ser deshabilitada, devolver un mensaje
                await LogActionAsync($"{MessagesLogsConstant.ACCOUNT_DISABLED_ERROR}");
                return new ResponseDto<ChartAccountResponseDto>
                {
                    Status = false,
                    StatusCode = 400,
                    Message = "La cuenta no puede ser deshabilitada debido a que tiene saldo o cuentas hijas habilitadas."
                };
            }
        }


        private async Task LogActionAsync(string action)
        {
            var logEntity = new LogEntity
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                Action = action,
                UserId = _auditService.GetUserId()
            };

            await _logContext.Logs.AddAsync(logEntity);
            await _logContext.SaveChangesAsync();
        }

        //metodos internos que se usan

        // Función para derivar el número de cuenta que se esta creando
        private async Task<string> DeriveAccountNumberAsync(string parentAccountNumber)
        {
            var parentAccount = await _transactionalContext.ChartAccounts
                                                           .FirstOrDefaultAsync(x => x.AccountNumber == parentAccountNumber);

            if (parentAccount == null)
            {
                return null;
            }

            var siblingAccounts = await _transactionalContext.ChartAccounts
                                                             .Where(x => x.ParentAccountNumber == parentAccountNumber)
                                                             .ToListAsync();

            if (!siblingAccounts.Any())
            {
                return $"{parentAccountNumber}01";
            }

            //obtener el ultimo numero de las cuentas hijas
            var lastAccount = siblingAccounts.OrderByDescending(x => x.AccountNumber).FirstOrDefault();

            // Verificar si se puede derivar un número válido
            if (lastAccount == null || string.IsNullOrEmpty(lastAccount.AccountNumber))
            {
                return null; // No se puede derivar el número
            }

            // Derivar el siguiente número de cuenta
            string lastAccountNumber = lastAccount.AccountNumber;
            string baseAccountNumber = lastAccountNumber.Substring(0, parentAccountNumber.Length);
            string lastSequence = lastAccountNumber.Substring(parentAccountNumber.Length);

            if (int.TryParse(lastSequence, out int numericSequence))
            {
                int nextSequence = numericSequence + 1;
                return $"{baseAccountNumber}{nextSequence:D2}";
            }

            return $"{parentAccountNumber}01";
        }

        // Función para validar si el nombre de la cuenta es único dentro de la misma clase padre
        private async Task<bool> IsAccountNameUniqueAsync(string parentAccountNumber, string accountName)
        {
            var existingAccount = await _transactionalContext.ChartAccounts
                                                            .FirstOrDefaultAsync(x => x.ParentAccountNumber == parentAccountNumber && x.Name == accountName);
            return existingAccount == null;
        }

               
    }

}
