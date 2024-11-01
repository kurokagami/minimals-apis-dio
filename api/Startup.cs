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
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Enuns;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Domain.ModelViews;
using MinimalApi.Domain.Services;
using MinimalApi.DTOs;
using MinimalApi.Infrasctrcuture.Db;

public class Startup
{
    public required IConfiguration Configuration { get; set; }

    private readonly string key = "";

    public Startup(IConfiguration configuration)
    {
        if (configuration != null)
        {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        key = Configuration.GetSection("Jwt").ToString() ?? "";
        }
    }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();



        services.AddScoped<IAdministrator, AdministratorService>();
        services.AddScoped<IVehicle, VehicleService>();


        //Configura o Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "Jwt",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
        });


        // Configura o contexto para usar SQL Server
        services.AddDbContext<DataBaseContext>(
            options =>
            {
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")
                );
            }
        );
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Adm
            string TokenGenerateJwt(Administrator administrator)
            {

                if (string.IsNullOrEmpty(key)) return string.Empty;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var expirationDate = DateTime.UtcNow.AddDays(1);

                var claims = new List<Claim>()
    {
        new Claim("Email", administrator.Email),
        new Claim("Profile", administrator.Profile),
        new Claim(ClaimTypes.Role, administrator.Profile),
        // Adiciona o claim da data de expiração formatada
        new Claim("DataDeExpiração", expirationDate.ToString("yyyy/MM/dd HH:mm:ss")),
    };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: expirationDate,
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            ValidationErrorsMsg validaAdmDTO(AdministratorDTO administratorDTO)
            {

                var validation = new ValidationErrorsMsg
                {
                    Messages = new List<string>()
                };

                if (string.IsNullOrEmpty(administratorDTO.Email))
                    validation.Messages.Add("Email não pode ser vazio");

                if (string.IsNullOrEmpty(administratorDTO.Password))
                    validation.Messages.Add("Senha não pode ser vazia");

                if (administratorDTO.Profile == null)
                    validation.Messages.Add("Perfil não pode ser vazio");

                return validation;

            }

            endpoints.MapPost("/administrators/login", ([FromBody] LoginDTO loginDTO, IAdministrator admService) =>
            {
                var adm = admService.Login(loginDTO);
                if (adm != null)
                {
                    string token = TokenGenerateJwt(adm);
                    return Results.Ok(new AdministratorLogged
                    {
                        Email = adm.Email,
                        Profile = adm.Profile,
                        Token = token
                    });
                }
                else
                {
                    return Results.Unauthorized();
                }
            }).AllowAnonymous().WithTags("Adm");

            endpoints.MapGet("/administrators/all", ([FromQuery] int? page, IAdministrator admService) =>
            {
                var adms = new List<AdministratorModelView>();
                var administrators = admService.All(page);
                foreach (var adm in administrators)
                {
                    adms.Add(new AdministratorModelView
                    {
                        Id = adm.Id,
                        Email = adm.Email,
                        Profile = adm.Profile
                    });
                }
                return Results.Ok(adms);

            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Adm");

            endpoints.MapGet("/administrators/{id}", ([FromRoute] int id, IAdministrator admService) =>
            {

                var adm = admService.FindForID(id);

                if (adm == null) return Results.NotFound();

                return Results.Ok(adm);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Adm");

            endpoints.MapPost("/administrators/add", ([FromBody] AdministratorDTO admDTO, IAdministrator admService) =>
            {

                var validation = validaAdmDTO(admDTO);
                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                if (!admDTO.Profile.HasValue || (admDTO.Profile != ProfileEnum.Adm && admDTO.Profile != ProfileEnum.Editor))
                {
                    return Results.BadRequest("O Profile deve ser 'Adm(0)' ou 'Editor(1)'.");
                }
                var adm = new Administrator
                {
                    Email = admDTO.Email,
                    Password = admDTO.Password,
                    Profile = admDTO.Profile.ToString() ?? ProfileEnum.Editor.ToString()
                };

                admService.Include(adm);

                return Results.Created($"/adm/{adm.Id}", adm);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Adm");
            #endregion

            #region Vehicles

            ValidationErrorsMsg validaVehicleDTO(VehicleDTO vehicleDTO)
            {

                var validation = new ValidationErrorsMsg
                {
                    Messages = new List<string>()
                };

                if (string.IsNullOrEmpty(vehicleDTO.Name))
                    validation.Messages.Add("O nome não pode ser vazio");

                if (string.IsNullOrEmpty(vehicleDTO.Brand))
                    validation.Messages.Add("A marca não pode ser vazia");

                if (vehicleDTO.Year < 1950)
                    validation.Messages.Add("O ano do veiculo é invalido, somente acima de 1950");

                return validation;

            }


            endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO vehicleDTO, IVehicle vehicleService) =>
            {

                var validation = validaVehicleDTO(vehicleDTO);
                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);

                var vehicle = new Vehicle
                {
                    Name = vehicleDTO.Name,
                    Brand = vehicleDTO.Brand,
                    Year = vehicleDTO.Year
                };

                vehicleService.Include(vehicle);

                return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicles");

            endpoints.MapGet("/vehicles", ([FromQuery] int? page, IVehicle vehicleService) =>
            {

                var vehicles = vehicleService.All(page);

                return Results.Ok(vehicles);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicles");

            endpoints.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicle vehicleService) =>
            {

                var vehicle = vehicleService.FindForID(id);

                if (vehicle == null) return Results.NotFound();

                return Results.Ok(vehicle);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Vehicles");

            endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicle vehicleService) =>
            {

                var vehicle = vehicleService.FindForID(id);

                if (vehicle == null) return Results.NotFound();

                var validation = validaVehicleDTO(vehicleDTO);
                if (validation.Messages.Count > 0)
                    return Results.BadRequest(validation);


                vehicle.Name = vehicleDTO.Name;
                vehicle.Brand = vehicleDTO.Brand;
                vehicle.Year = vehicleDTO.Year;

                vehicleService.Update(vehicle);

                return Results.Ok(vehicle);
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicles");

            endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicle vehicleService) =>
            {

                var vehicle = vehicleService.FindForID(id);

                if (vehicle == null) return Results.NotFound();

                vehicleService.Delete(vehicle);

                return Results.NoContent();
            }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicles");
            #endregion
        });
    }
}