using AutoMapper;
using ExamenU2LP.Constants;
using ExamenU2LP.Databases.LogDatabase;
using ExamenU2LP.Databases.LogDatabase.Entities;
using ExamenU2LP.Databases.TransactionalDatabase;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Dtos.Common;
using ExamenU2LP.Dtos.Entries;
using ExamenU2LP.Dtos.EntriesDetails;
using ExamenU2LP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExamenU2LP.Services;

public class EntriesService : IEntriesService
{
    private readonly TransactionalContext _transactionalContext;
    private readonly LogContext _logContext;
    private readonly IMapper _mapper;
    private readonly ILogger<EntriesService> _logger;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;

    public EntriesService(
            TransactionalContext transactionalContext,
            LogContext logContext,
            IMapper mapper,
            ILogger<EntriesService> logger,
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
    }

    //Metodos GET
    //Obtener todas las partidas contables, requiere paginación
    //Obtener partidas por rango de fechas, podria requerir paginacion
    //Buscar una partida contable (por id)
    //Buscar una partida contable por id usuario quien la creo

    //Metodo POST
    public async Task<ResponseDto<EntryResponseDto>> CreateEntryAsync (EntryCreateDto dto)
    {
        using(var transaction = await _transactionalContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Primero se debe validar que las cuentas que se manden en los arreglos existan
                var accountNumbers = dto.DebitAccounts.Select(d => d.AccountNumber)
                                  .Concat(dto.CreditAccounts.Select(c => c.AccountNumber))
                                  .Distinct();

                var existingAccounts = await _transactionalContext.ChartAccounts
                                  .Where(a => accountNumbers.Contains(a.AccountNumber))
                                  .ToListAsync();

                //si la cantidad de cuentas que se mandan no coincide con la cantidad verificada de cuentas que existen entonces hay un error
                if (existingAccounts.Count != accountNumbers.Count())
                {
                    //Esta verificacion si funciona
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "Algunos AccountNumbers no existen en ChartAccounts"
                    };
                }

                //Validar que todas las cuentas tenga AllowsMovement en True e IsDisabled en false
                var invalidAccounts = existingAccounts
                         .Where(a => !a.AllowsMovement || a.IsDisabled)
                         .Select(a => a.AccountNumber)
                         .ToList();

                if (invalidAccounts.Any())
                {
                    //Esta verificacion si funciona
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "Algunas cuentas no permiten movimientos o están deshabilitadas"
                    };
                }

                //Validar los valores de las posiciones de las cuentas sean "Debe" o "Haber"
                bool hasInvalidPositions = dto.DebitAccounts.Any(d => d.EntryPosition != "Debe") ||
                           dto.CreditAccounts.Any(c => c.EntryPosition != "Haber");
                bool hasInvalidAmounts = dto.DebitAccounts.Any(d => d.Amount <= 0) ||
                                         dto.CreditAccounts.Any(c => c.Amount <= 0);

                if (hasInvalidPositions || hasInvalidAmounts)
                {
                    //Esta verificacion si funciona
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "Valores de EntryPosition inválidos o montos no positivos"
                    };
                }

                //Validar que el total de debitos sea igual al total de creditos
                decimal totalDebit = dto.DebitAccounts.Sum(d => d.Amount);
                decimal totalCredit = dto.CreditAccounts.Sum(c => c.Amount);

                if (totalDebit != totalCredit)
                {
                    //Esta verificacion si funciona
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "El total de débitos no es igual al total de créditos"
                    };
                }

                //Hay que hacer validacion que los AccountNumbers no se repitan en los creditos y debidos
                var allAccountNumbers = dto.DebitAccounts.Select(d => d.AccountNumber)
                       .Concat(dto.CreditAccounts.Select(c => c.AccountNumber));
                if (allAccountNumbers.Distinct().Count() != allAccountNumbers.Count())
                {
                    //Esta verificacion si funciona
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 400,
                        Message = "Cada AccountNumber debe aparecer solo una vez en Debe o Haber"
                    };
                }

                // Una vez pasada todas estas validaciones se puede crear la partida contable y sus detalles
                var entry = new EntryEntity
                {
                    Date = dto.Date,
                    Description = dto.Description,
                    IsEditable = true,
                };
                await _transactionalContext.Entries.AddAsync(entry);
                await _transactionalContext.SaveChangesAsync();

                //Crear un registro para cada cuenta en la tabla EntryDetails
                var entryDetails = new List<EntryDetailEntity>();

                foreach (var debit in dto.DebitAccounts)
                {
                    entryDetails.Add(new EntryDetailEntity
                    {
                        EntryNumber = entry.EntryNumber,
                        AccountNumber = debit.AccountNumber,
                        EntryPosition = debit.EntryPosition,
                        Amount = debit.Amount,
                    });
                }

                foreach (var credit in dto.CreditAccounts)
                {
                    entryDetails.Add(new EntryDetailEntity
                    {
                        EntryNumber = entry.EntryNumber,
                        AccountNumber = credit.AccountNumber,
                        EntryPosition = credit.EntryPosition,
                        Amount = credit.Amount,
                    });
                }

                await _transactionalContext.EntryDetails.AddRangeAsync(entryDetails);
                await _transactionalContext.SaveChangesAsync();

                // Actualizar los saldos de las cuentas incluidas en la partida contable
                foreach (var detail in entryDetails)
                {
                    // Obtener el tipo de comportamiento de la cuenta
                    var account = await _transactionalContext.ChartAccounts
                                      .Include(a => a.BehaviorType)
                                      .FirstOrDefaultAsync(a => a.AccountNumber == detail.AccountNumber);

                    if (account == null)
                    {
                        await LogActionAsync($"Error: La cuenta {detail.AccountNumber} no existe en ChartAccounts.");
                        continue; // Si no se encuentra la cuenta, omitir esta iteración
                    }

                    var accountBalance = await _transactionalContext.AccountBalances
                        .FirstOrDefaultAsync(ab => ab.AccountNumber == detail.AccountNumber &&
                                                   ab.Year == entry.Date.Year &&
                                                   ab.Month == entry.Date.Month);

                    //si por alguna extraña razon no existe el saldo de la cuenta para el mes y año crearlo iniciandolo en cero
                    if (accountBalance == null)
                    {
                        accountBalance = new AccountBalanceEntity
                        {
                            Id = Guid.NewGuid(),
                            AccountNumber = detail.AccountNumber,
                            Year = entry.Date.Year,
                            Month = entry.Date.Month,
                            Balance = 0,
                        };
                        await _transactionalContext.AccountBalances.AddAsync(accountBalance);
                    }

                    bool isDebitBehavior = account.BehaviorType.Type == "Debe";
                    bool isDebitEntryPosition = detail.EntryPosition == "Debe";

                    if (isDebitBehavior)
                    {
                        // Si el comportamiento de la cuenta es "Debe"
                        accountBalance.Balance += isDebitEntryPosition ? detail.Amount : -detail.Amount;
                    }
                    else
                    {
                        // Si el comportamiento de la cuenta es "Haber"
                        accountBalance.Balance += isDebitEntryPosition ? -detail.Amount : detail.Amount;
                    }
                }

                await _transactionalContext.SaveChangesAsync();


                //throw new Exception("Error para probar el rollback");
                await transaction.CommitAsync();


                //Se debe actualizar el saldo de las saldos de las cuentas padres por las cuentas que se acaban de ingresar en una nueva transaccion
                using (var updateTransaction = await _transactionalContext.Database.BeginTransactionAsync())
                {
                    foreach (var detail in entryDetails)
                    {
                        await UpdateParentBalancesAsync(detail.AccountNumber, entry.Date.Year, entry.Date.Month);
                    }

                    await updateTransaction.CommitAsync();
                }



                await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_SUCCESS}");

                //mapeo manual de DebitAccounts y CreditAccounts para el DTO
                var debitAccounts = entryDetails
                    .Where(ed => ed.EntryNumber == entry.EntryNumber && ed.EntryPosition == "Debe")
                    .Select(ed => new EntryDetailResponseDto
                    {
                        Id = ed.Id,
                        EntryNumber = ed.EntryNumber,
                        AccountNumber = ed.AccountNumber,
                        EntryPosition = ed.EntryPosition,
                        Amount = ed.Amount,
                    })
                    .ToList();

                var creditAccounts = entryDetails
                    .Where(ed => ed.EntryNumber == entry.EntryNumber && ed.EntryPosition == "Haber")
                    .Select(ed => new EntryDetailResponseDto
                    {
                    Id = ed.Id,
                    EntryNumber = ed.EntryNumber,
                    AccountNumber = ed.AccountNumber,
                    EntryPosition = ed.EntryPosition,
                    Amount = ed.Amount
                    })
                    .ToList();

                var responseDto = new EntryResponseDto
                {
                    EntryNumber = entry.EntryNumber,
                    Date = entry.Date,
                    Description = entry.Description,
                    IsEditable = entry.IsEditable,
                    DebitAccounts = debitAccounts,
                    CreditAccounts = creditAccounts
                };

                return new ResponseDto<EntryResponseDto>
                {
                    Status = true,
                    StatusCode = 201,
                    Message = "Partida contable creada exitosamente",
                    Data = responseDto
                };
            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();

                await LogActionAsync($"{MessagesLogsConstant.ENTRY_ADD_ERROR}");
                _logger.LogError(e, "Error al crear la partida contable");

                return new ResponseDto<EntryResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo error al crear la partida contable"
                };
            }
        }
    }

    //Metodos PUT 

    //Metodo para el manejo de los logs
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

    //Metodo para actualizar el saldo de las cuentas padres
    private async Task UpdateParentBalancesAsync(string accountNumber, int year, int month)
    {
        // Obtener la cuenta actual
        var account = await _transactionalContext.ChartAccounts
                         .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        // Condición de parada: si se llega a la cuenta base "No aplica"
        if (account == null || (account.ParentAccountNumber == "0" && account.Name == "No aplica"))
        {
            // Salir de la recursión si estamos en la cuenta base
            return;
        }

        // Calcular el nuevo saldo de la cuenta padre basado en las cuentas hijas
        var childAccounts = await _transactionalContext.AccountBalances
                            .Where(ab => ab.Account.ParentAccountNumber == account.ParentAccountNumber &&
                                         ab.Year == year && ab.Month == month)
                            .ToListAsync();

        decimal newParentBalance = childAccounts.Sum(ca => ca.Balance);

        // Obtener o crear el balance de la cuenta padre
        var parentBalance = await _transactionalContext.AccountBalances
                          .FirstOrDefaultAsync(pb => pb.AccountNumber == account.ParentAccountNumber &&
                                                     pb.Year == year && pb.Month == month);

        //probar a comentar esta parte, al final todas las cuentas existen y la que no pues que no se implemente
        if (parentBalance == null)
        {
            parentBalance = new AccountBalanceEntity
            {
                Id = Guid.NewGuid(),
                AccountNumber = account.ParentAccountNumber,
                Year = year,
                Month = month,
                Balance = newParentBalance,
            };
            await _transactionalContext.AccountBalances.AddAsync(parentBalance);
        }
        else
        {
            parentBalance.Balance = newParentBalance;
        }

        await _transactionalContext.SaveChangesAsync();

        // Llamar a la función de nuevo con la cuenta padre
        await UpdateParentBalancesAsync(account.ParentAccountNumber, year, month);
    }

}