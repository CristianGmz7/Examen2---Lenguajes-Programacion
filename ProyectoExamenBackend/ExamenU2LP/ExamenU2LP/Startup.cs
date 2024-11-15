using ExamenU2LP.Databases.LogDatabase;
using ExamenU2LP.Databases.TransactionalDatabase;
using ExamenU2LP.Databases.TransactionalDatabase.Entities;
using ExamenU2LP.Helpers;
using ExamenU2LP.Services;
using ExamenU2LP.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ExamenU2LP;

public class Startup
{
    private readonly IConfiguration Configuration;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        //HttpContext para el uso de Tokens
        services.AddHttpContextAccessor();

        // Add DbContext
        services.AddDbContext<TransactionalContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<LogContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("LogConnection")));

        //Servicios de interfaces que se creen
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IAuditService, AuditService>();
        services.AddTransient<IEntriesService, EntriesService>();

        //servicios de los logs irian aqui tambien

        //Identity
        services.AddIdentity<UserEntity, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        }).AddEntityFrameworkStores<TransactionalContext>()
            .AddDefaultTokenProviders();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidAudience = Configuration["JWT:ValidAudience"],
                ValidIssuer = Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))    //llave 
            };
        });

        //automapper
        services.AddAutoMapper(typeof(AutoMapperProfile));

        //cors (habilitar cuando se haga conexion con frontend)
        services.AddCors(opt =>
        {
            var allowURLS = Configuration.GetSection("AllowURLS").Get<string[]>();

            opt.AddPolicy("CorsPolicy", builder => builder
            .WithOrigins(allowURLS)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.UseRouting();  //useRouting

        app.UseCors("CorsPolicy");        //(habilitar cuando se haga conexion con frontend)

        app.UseAuthentication();    //middleWare
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
