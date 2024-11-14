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
    private readonly int PAGE_SIZE;

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
        PAGE_SIZE = configuration.GetValue<int>("Pagination:EntryPageSize");
    }

    //Metodos GET
    //Se podria probar a implementar la parte de Log similar a la del Login
    //Obtener todas las partidas contables, requiere paginación
    public async Task<ResponseDto<PaginationDto<List<EntryResponseDto>>>> GetEntriesListAsync(
            int page = 1
        )
    {
        int startIndex = (page - 1) * PAGE_SIZE;

        var entryEntityQuery = _transactionalContext.Entries
            .Include(e => e.Details)
            .Where(e => e.EntryNumber == e.EntryNumber);

        int totalEntries = await entryEntityQuery.CountAsync();

        int totalPages = (int)Math.Ceiling((double)totalEntries / PAGE_SIZE);

        var entriesEntity = await _transactionalContext.Entries
            .Include(e => e.Details)
            .OrderByDescending(x => x.EntryNumber)          //probar si dejar OrderByDescending o OrderBy
            .Skip(startIndex)
            .Take(PAGE_SIZE)
            .ToListAsync();

        var entriesDtos = entriesEntity.Select(entry => new EntryResponseDto
        {
            EntryNumber = entry.EntryNumber,
            Date = entry.Date,
            Description = entry.Description,
            IsEditable = entry.IsEditable,

            DebitAccounts = entry.Details
                .Where(d => d.EntryPosition == "Debe")
                .Select(d => new EntryDetailResponseDto
                {
                    Id = d.Id,
                    EntryNumber = d.EntryNumber,
                    AccountNumber = d.AccountNumber,
                    EntryPosition = d.EntryPosition,
                    Amount = d.Amount
                }).ToList(),

            CreditAccounts = entry.Details
            .Where(d => d.EntryPosition == "Haber")
            .Select(d => new EntryDetailResponseDto
            {
                Id = d.Id,
                EntryNumber = d.EntryNumber,
                AccountNumber = d.AccountNumber,
                EntryPosition = d.EntryPosition,
                Amount = d.Amount
            }).ToList()

        }).ToList();

        //PARA QUE ESTE METODO FUNCIONE TIENE QUE ESTAR UN TOKEN ACTIVO, O SEA ESTAR LOGUEADO
        //Por lo que por el momento dejarlo comentado y dejar consultas anonimas para el mapeo y todo eso
        //Cuando ya se implementé el token descomentarlo 

        //await LogActionAsync($"{MessagesLogsConstant.ENTRY_SEARCH_SUCCESS}");     
        return new ResponseDto<PaginationDto<List<EntryResponseDto>>>
        {
            StatusCode = 200,
            Status = true,
            Message = "Partidas encontradas satisfactoriamente",
            Data = new PaginationDto<List<EntryResponseDto>>
            {
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                TotalItems = totalEntries,
                TotalPages = totalPages,
                Items = entriesDtos,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages,
            }
        };
    }


    //Buscar una partida contable (por id)
    public async Task<ResponseDto<EntryResponseDto>> GetEntryByIdAsync (int entryNumber)
    {
        var entryEntity = await _transactionalContext.Entries
            .Include(e => e.Details) 
            .FirstOrDefaultAsync(e => e.EntryNumber == entryNumber);

        if (entryEntity == null)
        {
            //Por el momento los logs los endpoints de obtener estaran bloqueados por el GetUserId del metodo y por habilitar Token en el bruno
            //await LogActionAsync($"{MessagesLogsConstant.ENTRY_SEARCH_ERROR}");
            return new ResponseDto<EntryResponseDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "No se encontró la partida contable"
            };
        }

        var entryDto = new EntryResponseDto
        {
            EntryNumber = entryEntity.EntryNumber,
            Date = entryEntity.Date,
            Description = entryEntity.Description,
            IsEditable = entryEntity.IsEditable,

            // Mapear los detalles de la partida a DebitAccounts y CreditAccounts
            DebitAccounts = entryEntity.Details
                .Where(d => d.EntryPosition == "Debe")
                .Select(d => new EntryDetailResponseDto
                {
                    Id = d.Id,
                    EntryNumber = d.EntryNumber,
                    AccountNumber = d.AccountNumber,
                    EntryPosition = d.EntryPosition,
                    Amount = d.Amount
                }).ToList(),

            CreditAccounts = entryEntity.Details
                .Where(d => d.EntryPosition == "Haber")
                .Select(d => new EntryDetailResponseDto
                {
                    Id = d.Id,
                    EntryNumber = d.EntryNumber,
                    AccountNumber = d.AccountNumber,
                    EntryPosition = d.EntryPosition,
                    Amount = d.Amount
                }).ToList()
        };

        //await LogActionAsync($"{MessagesLogsConstant.ENTRY_SEARCH_SUCCESS}");

        return new ResponseDto<EntryResponseDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Partida contable encontrada exitosamente",
            Data = entryDto
        };
    }

    //Obtener partidas por rango de fechas, podria requerir paginacion
    //Buscar una partida contable por id usuario quien la creo, hay que ver como se comporta por el Token y el AllowAnonymous y el _getByid

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
                        continue; // Si no se encuentra la cuenta pasarse al siguiente elemento
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
    public async Task<ResponseDto<EntryResponseDto>> EditEntryAsync (EntryEditDto dto, int entryNumber)
    {
        using (var transaction = await _transactionalContext.Database.BeginTransactionAsync())
        {
            try
            {
                var entryEntity = await _transactionalContext.Entries.FirstOrDefaultAsync(e => e.EntryNumber == entryNumber);

                if (entryEntity == null)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_UPDATE_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 404,
                        Message = "No se encontro el registro"
                    };
                }

                //Verificar que la partida que se quiera modificar se pueda modificar
                if (!entryEntity.IsEditable)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_UPDATE_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 404,
                        Message = "No se puede editar este registro porque fue dado de baja"
                    };
                }

                entryEntity.Description = dto.Description;

                _transactionalContext.Entries.Update(entryEntity);
                await _transactionalContext.SaveChangesAsync();

                //throw new Exception("Error para probar el rollback");
                await transaction.CommitAsync();

                await LogActionAsync($"{MessagesLogsConstant.ENTRY_UPDATE_SUCCESS}");

                var responseDto = new EntryResponseDto
                {
                    EntryNumber = entryEntity.EntryNumber,
                    Date = entryEntity.Date,
                    Description = entryEntity.Description,
                    IsEditable = entryEntity.IsEditable
                };

                return new ResponseDto<EntryResponseDto>
                {
                    Status = true,
                    StatusCode = 200,
                    Message = "Partida contable editada exitosamente",
                    Data = responseDto
                };
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();

                await LogActionAsync($"{MessagesLogsConstant.ENTRY_UPDATE_ERROR}");
                _logger.LogError(e, "Error al editar la partida contable");

                return new ResponseDto<EntryResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo error al editar la partida contable"
                };
            }
        }
    }

    //metodo de dar de baja
    public async Task<ResponseDto<EntryResponseDto>> WriteOff (int entryNumber)
    {
        using (var transaction = await _transactionalContext.Database.BeginTransactionAsync())
        {
            try
            {
                var entryEntity = await _transactionalContext.Entries
                      .Include(e => e.Details) // cargar los detalles para cuando se necesiten para generar la contrapartida
                      .FirstOrDefaultAsync(e => e.EntryNumber == entryNumber);

                if (entryEntity == null)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_REVERSED_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 404,
                        Message = "No se encontro el registro"
                    };
                }

                //Verificar que la partida que se quiera dar de baja no sea una partida que ya este dada de baja
                if (!entryEntity.IsEditable)
                {
                    await LogActionAsync($"{MessagesLogsConstant.ENTRY_REVERSED_ERROR}");
                    return new ResponseDto<EntryResponseDto>
                    {
                        Status = false,
                        StatusCode = 404,
                        Message = "No se puede dar de baja este registro porque fue dado de baja"
                    };
                }

                //Hay que hacer que la partida original ya no sea editable
                entryEntity.IsEditable = false;
                _transactionalContext.Entries.Update(entryEntity);

                // Creación de la contrapartida
                //Verificar que se haya generado un numero
                var newEntry = new EntryEntity
                {
                    Date = DateTime.Now,
                    Description = $"Contrapartida de la partida {entryNumber}",
                    IsEditable = false
                };

                await _transactionalContext.Entries.AddAsync(newEntry);
                await _transactionalContext.SaveChangesAsync();

                //La contrapartida tiene las mismas cuentas y cantidades de la partida pero invertidos
                var newEntryDetails = entryEntity.Details.Select(detail => new EntryDetailEntity
                {
                    EntryNumber = newEntry.EntryNumber,
                    AccountNumber = detail.AccountNumber,
                    EntryPosition = detail.EntryPosition == "Debe" ? "Haber" : "Debe",
                    Amount = detail.Amount
                }).ToList();

                await _transactionalContext.EntryDetails.AddRangeAsync(newEntryDetails);
                await _transactionalContext.SaveChangesAsync();

                // Actualizar los saldos de las cuentas de la contrapartida
                foreach (var detail in newEntryDetails)
                {
                    var account = await _transactionalContext.ChartAccounts
                                      .Include(a => a.BehaviorType)
                                      .FirstOrDefaultAsync(a => a.AccountNumber == detail.AccountNumber);

                    if (account == null)
                    {
                        continue;
                    }

                    var accountBalance = await _transactionalContext.AccountBalances
                        .FirstOrDefaultAsync(ab => ab.AccountNumber == detail.AccountNumber &&
                                                   ab.Year == newEntry.Date.Year &&
                                                   ab.Month == newEntry.Date.Month);

                    if (accountBalance == null)
                    {
                        accountBalance = new AccountBalanceEntity
                        {
                            Id = Guid.NewGuid(),
                            AccountNumber = detail.AccountNumber,
                            Year = newEntry.Date.Year,
                            Month = newEntry.Date.Month,
                            Balance = 0,
                        };
                        await _transactionalContext.AccountBalances.AddAsync(accountBalance);
                    }

                    bool isDebitBehavior = account.BehaviorType.Type == "Debe";
                    bool isDebitEntryPosition = detail.EntryPosition == "Debe";

                    if (isDebitBehavior)
                    {
                        accountBalance.Balance += isDebitEntryPosition ? detail.Amount : -detail.Amount;
                    }
                    else
                    {
                        accountBalance.Balance += isDebitEntryPosition ? -detail.Amount : detail.Amount;
                    }
                }

                await _transactionalContext.SaveChangesAsync();

                //Confirmar la transaccion para que los saldos sean actualizados correctamente
                //throw new Exception("Error para probar el rollback");
                await transaction.CommitAsync();

                //Actualizar saldos de las cuentas padres
                using (var updateTransaction = await _transactionalContext.Database.BeginTransactionAsync())
                {
                    foreach (var detail in newEntryDetails)
                    {
                        await UpdateParentBalancesAsync(detail.AccountNumber, newEntry.Date.Year, newEntry.Date.Month);
                    }

                    await updateTransaction.CommitAsync();
                }

                await LogActionAsync($"{MessagesLogsConstant.ENTRY_REVERSED_SUCCESS}");

                //Mapeo del arreglo de cuentas del credito y debito de la contrapartida
                var debitAccounts = newEntryDetails
                    .Where(ed => ed.EntryPosition == "Debe")
                    .Select(ed => new EntryDetailResponseDto
                    {
                        Id = ed.Id,
                        EntryNumber = ed.EntryNumber,
                        AccountNumber = ed.AccountNumber,
                        EntryPosition = ed.EntryPosition,
                        Amount = ed.Amount,
                    })
                    .ToList();

                var creditAccounts = newEntryDetails
                    .Where(ed => ed.EntryPosition == "Haber")
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
                    EntryNumber = newEntry.EntryNumber,
                    Date = newEntry.Date,
                    Description = newEntry.Description,
                    IsEditable = newEntry.IsEditable,
                    DebitAccounts = debitAccounts,
                    CreditAccounts = creditAccounts
                };

                return new ResponseDto<EntryResponseDto>
                {
                    Status = true,
                    StatusCode = 201,
                    Message = "Contrapartida creada exitosamente",
                    Data = responseDto
                };
            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();

                await LogActionAsync($"{MessagesLogsConstant.ENTRY_REVERSED_ERROR}");
                _logger.LogError(e, "Error al revertir la partida contable");

                return new ResponseDto<EntryResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Se produjo error al revertir la partida contable"
                };
            }
        }
    }

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

        // Condición de parada cuando se llega a la cuenta base "No aplica"
        if (account == null || (account.ParentAccountNumber == "0" && account.Name == "No aplica"))
        {
            return;
        }

        // Calcular el nuevo saldo de la cuenta padre basado en las cuentas hijas
        var childAccounts = await _transactionalContext.AccountBalances
                            .Where(ab => ab.Account.ParentAccountNumber == account.ParentAccountNumber &&
                                         ab.Year == year && ab.Month == month)
                            .ToListAsync();

        decimal newParentBalance = childAccounts.Sum(ca => ca.Balance);

        // Evitar la creación de balance para la cuenta base "0" de No Aplica
        if (account.ParentAccountNumber == "0")
        {
            return; 
        }

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