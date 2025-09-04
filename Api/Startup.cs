
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.Enum;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Service;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "";

    }

    private string key = "";

    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option => {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option => {
            option.TokenValidationParameters = new TokenValidationParameters{
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IVehicleService, VehicleService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui!"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddDbContext<DatabaseContext>(options => {
            options.UseSqlServer(
                Configuration.GetConnectionString("SqlServer") //,
            // ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("sqlserver"))
            );
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors();

        app.UseEndpoints(endpoints => {
        
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administrators
            string GerarTokenJwt(Administrator administrator){
                if(string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrator.Email),
                    new Claim("Perfil", administrator.Perfil),
                    new Claim(ClaimTypes.Role, administrator.Perfil) 
                };
                
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administrators/login", ( [FromBody] LoginDTO loginDTO, IAdministratorService AdministratorService ) => {
                var adm = AdministratorService.Login(loginDTO);
                if(adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministratorLogged
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }
                else
                    return Results.Unauthorized();
            }).AllowAnonymous().WithTags("Administradores");

            endpoints.MapGet("/administrators", ( [FromQuery] int? pagina, IAdministratorService AdministratorService ) => {
                var adms = new List<AdministratorModelView>();
                var administrators = AdministratorService.All(pagina);
                foreach(var adm in administrators)
                {
                    adms.Add(new AdministratorModelView{
                        Id = adm.Id,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }
                return Results.Ok(adms);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/administrators/{id}", ( [FromRoute] int id, IAdministratorService AdministratorService ) => {
                var administrator = AdministratorService.BuscaPorId(id);
                if(administrator == null) return Results.NotFound();
                return Results.Ok(new AdministratorModelView{
                        Id = administrator.Id,
                        Email = administrator.Email,
                        Perfil = administrator.Perfil
                });
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapPost("/administrators", ( [FromBody] AdministratorDTO administratorDTO, IAdministratorService AdministratorService ) => {
                var validation = new ValidationErrors{
                    Messages = new List<string>()
                };

                if(string.IsNullOrEmpty(administratorDTO.Email))
                    validation.Messages.Add("Digite um e-mail válido.");
                if(string.IsNullOrEmpty(administratorDTO.Senha))
                    validation.Messages.Add("Digite uma senha válida.");
                if(administratorDTO.Perfil == null)
                    validation.Messages.Add("Perfil inválido.");

                if(validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                var administrator = new Administrator{
                    Email = administratorDTO.Email,
                    Senha = administratorDTO.Senha,
                    Perfil = administratorDTO.Perfil?.ToString() ?? Perfil.Editor.ToString()
                };

                AdministratorService.Incluir(administrator);

                return Results.Created($"/administrator/{administrator.Id}", new AdministratorModelView{
                    Id = administrator.Id,
                    Email = administrator.Email,
                    Perfil = administrator.Perfil
                });

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");
            #endregion

            #region Vehicles
            ValidationErrors validationDTO(VehicleDTO vehicleDTO)
            {
                var validation = new ValidationErrors{
                    Messages = new List<string>()
                };

                if(string.IsNullOrEmpty(vehicleDTO.Nome))
                    validation.Messages.Add("O nome do veículo é um dado obrigatório.");

                if(string.IsNullOrEmpty(vehicleDTO.Marca))
                    validation.Messages.Add("A marca do veículo é um dado obrigatório.");

                if(vehicleDTO.Ano < 1950)
                    validation.Messages.Add("Veículo antigo! Aceito somente anos superiores a 1950.");

                return validation;
            }

            endpoints.MapPost("/vehicles", ( [FromBody] VehicleDTO vehicleDTO, IVehicleService VehicleService ) => {
                var validation = validationDTO(vehicleDTO);
                if(validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                var vehicle = new Vehicle{
                    Nome = vehicleDTO.Nome,
                    Marca = vehicleDTO.Marca,
                    Ano = vehicleDTO.Ano
                };
                VehicleService.Incluir(vehicle);

                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veículos");

            endpoints.MapGet("/vehicles", ( [FromQuery] int? pagina, IVehicleService VehicleService ) => {
                var vehicles = VehicleService.All(pagina);

                return Results.Ok(vehicles);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veículos");

            endpoints.MapGet("/vehicles/{id}", ( [FromRoute] int id, IVehicleService VehicleService ) => {
                var vehicle = VehicleService.BuscaPorId(id);
                if(vehicle == null) return Results.NotFound();
                return Results.Ok(vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
            .WithTags("Veículos");

            endpoints.MapPut("/vehicles/{id}", ( [FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService VehicleService ) => {
                var vehicle = VehicleService.BuscaPorId(id);
                if(vehicle == null) return Results.NotFound();

                var validation = validationDTO(vehicleDTO);
                if(validation.Messages.Count > 0)
                    return Results.BadRequest(validation);
                
                vehicle.Nome = vehicleDTO.Nome;
                vehicle.Marca = vehicleDTO.Marca;
                vehicle.Ano = vehicleDTO.Ano;

                VehicleService.Atualizar(vehicle);
                
                return Results.Ok(vehicle);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veículos");

            endpoints.MapDelete("/vehicles/{id}", ( [FromRoute] int id, IVehicleService VehicleService ) => {
                var vehicle = VehicleService.BuscaPorId(id);
                if(vehicle == null) return Results.NotFound();

                VehicleService.Apagar(vehicle);
                
                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veículos");
            #endregion
        });
    }
}