using ExamenU2LP.Constants;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace ExamenU2LP.Databases.TransactionalDatabase;

public class TransactionalSeeder
{
    public static async Task LoadDataAsync(
            TransactionalContext transactionalContext,
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory
        )
    {
        try
        {
            await LoadRolesAndUsersAsync(userManager, roleManager, loggerFactory);
            await LoadAccounBehaviorsTypes(loggerFactory, transactionalContext);
            await LoadChartAccounts(loggerFactory, transactionalContext);
            await LoadAccountBalances(loggerFactory, transactionalContext);
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<TransactionalSeeder>();
            logger.LogError(e, "Error inicializando la data del API");
        }
    }

    public static async Task LoadRolesAndUsersAsync(
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory
        )
    {
        try
        {
            //LOGICA PARA IMPLEMENTAR ROLES Y USUARIOS
            if(!await roleManager.Roles.AnyAsync())
            {
                await roleManager.CreateAsync(new IdentityRole(RolesConstant.USER));
            }

            if(!await userManager.Users.AnyAsync())
            {
                var user1 = new UserEntity
                {
                    FirstName = "Usuario1",
                    LastName = "ExamenU2",
                    Email = "user1@examenu2.edu",
                    UserName = "user1@examenu2.edu",
                };

                var user2 = new UserEntity
                {
                    FirstName = "Usuario2",
                    LastName = "ExamenU2",
                    Email = "use21@examenu2.edu",
                    UserName = "user2@examenu2.edu",
                };

                await userManager.CreateAsync(user1, "Temporal01*");
                await userManager.CreateAsync(user2, "Temporal01*");

                await userManager.AddToRoleAsync(user1, RolesConstant.USER);
                await userManager.AddToRoleAsync(user2, RolesConstant.USER);
            }
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<TransactionalSeeder>();
            logger.LogError(e.Message);
        }
    }

    //Load de account_behaviors_types
    public static async Task LoadAccounBehaviorsTypes (
            ILoggerFactory loggerFactory,
            TransactionalContext transactionalContext
        )
    {
        try
        {
            var jsonFilePath = "SeedData/account_behavior_type.json";
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var accountBehaviorTypes = JsonConvert.DeserializeObject<List<AccountBehaviorTypeEntity>>(jsonContent);

            if(!await transactionalContext.AccountBehaviorTypes.AnyAsync())
            {
                var user = await transactionalContext.Users.FirstOrDefaultAsync();
                for (int i = 0; i < accountBehaviorTypes.Count; i++)
                {
                    accountBehaviorTypes[i].CreatedBy = user.Id;
                    accountBehaviorTypes[i].CreatedDate = DateTime.Now;
                    accountBehaviorTypes[i].UpdatedBy = user.Id;
                    accountBehaviorTypes[i].UpdatedDate = DateTime.Now;
                }

                transactionalContext.AddRange(accountBehaviorTypes);
                await transactionalContext.SaveChangesAsync();
            }
        }
        catch ( Exception e )
        {
            var logger = loggerFactory.CreateLogger<TransactionalSeeder>();
            logger.LogError(e, "Error al ejecutar el Seed de Tipo de comportamiento de cuentas");
        }
    }

    //Load de chart_accounts
    public static async Task LoadChartAccounts (
            ILoggerFactory loggerFactory,
            TransactionalContext transactionalContext
        )
    {
        try
        {
            var jsonFilePath = "SeedData/chart_accounts.json";
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var chartAccounts = JsonConvert.DeserializeObject<List<ChartAccountEntity>>(jsonContent);

            if (!await transactionalContext.ChartAccounts.AnyAsync())
            {
                var user = await transactionalContext.Users.FirstOrDefaultAsync();
                for (int i = 0; i < chartAccounts.Count; i++)
                {
                    chartAccounts[i].CreatedBy = user.Id;
                    chartAccounts[i].CreatedDate = DateTime.Now;
                    chartAccounts[i].UpdatedBy = user.Id;
                    chartAccounts[i].UpdatedDate = DateTime.Now;
                }

                transactionalContext.AddRange(chartAccounts);
                await transactionalContext.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<TransactionalSeeder>();
            logger.LogError(e, "Error al ejecutar el Seed del Catalogo de cuentas");
        }
    }

    //Load de account_balances
    public static async Task LoadAccountBalances ( 
            ILoggerFactory loggerFactory, 
            TransactionalContext transactionalContext 
        )
    {
        try
        {
            var jsonFilePath = "SeedData/account_balances2.json";
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var accountBalances = JsonConvert.DeserializeObject<List<AccountBalanceEntity>>(jsonContent);

            if (!await transactionalContext.AccountBalances.AnyAsync())
            {
                var user = await transactionalContext.Users.FirstOrDefaultAsync();
                for (int i = 0; i < accountBalances.Count; i++)
                {
                    accountBalances[i].Id = new Guid();
                    accountBalances[i].CreatedBy = user.Id;
                    accountBalances[i].CreatedDate = DateTime.Now;
                    accountBalances[i].UpdatedBy = user.Id;
                    accountBalances[i].UpdatedDate = DateTime.Now;
                }

                transactionalContext.ChangeTracker.Clear();
                transactionalContext.AddRange(accountBalances);
                await transactionalContext.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<TransactionalSeeder>();
            logger.LogError(e, "Error al ejecutar el Seed de Saldos de cuentas");
        }
    }
}
