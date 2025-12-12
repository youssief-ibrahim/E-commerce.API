using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using E_commerce.API.Filtter;
using E_commerce.API.Middlewares;
using E_commerce.Core.Basic;
using E_commerce.Core.Helper;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using E_commerce.EF.Data;
using E_commerce.EF.Localization;
using E_commerce.EF.Reposatory;
using E_commerce.EF.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace E_commerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.AllowAnyOrigin();
                                      policy.AllowAnyMethod();
                                      policy.AllowAnyHeader();
                                  });
            });
            var connect = builder.Configuration.GetConnectionString("defultconnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseLazyLoadingProxies().UseSqlServer(connect)); //using  lazyloding
            options.UseSqlServer(connect));
            // without lazyloding
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(option =>
            {
                option.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            builder.Services.AddScoped(typeof(IGenericReposatory<>), typeof(GenericReposatory<>));

            builder.Services.AddScoped<ITokenReposatory, TokenReposatory>();

            builder.Services.AddAutoMapper(typeof(HelperMapper).Assembly);

            builder.Services.AddScoped<ResponseHandler>();

            builder.Services.AddScoped<IEmailService, EmailSender>();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermetionPolicyProvider>();

            builder.Services.AddScoped<IAuthorizationHandler, PermetionAuthrizationHandelar>();

            builder.Services.Configure<SecurityStampValidatorOptions>(option =>
            option.ValidationInterval = TimeSpan.Zero
           );

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(option => // vervifed
            {
                option.RequireHttpsMetadata = true;
                option.SaveToken = true;  // still token is valied
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuerIP"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudienceIP"],
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            #region Authcation in Swagger

            builder.Services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "E-CommerceAPI",
                    Description = "Api for E-Commerce"
                });
                swagger.EnableAnnotations();
                // To Enable authorization using Swagger (JWT)
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the tex"
                });

                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                        new OpenApiSecurityScheme
                        {
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
            #endregion

            builder.Services.AddLocalization();

            builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

            builder.Services.AddMvc().AddDataAnnotationsLocalization(option =>
              option.DataAnnotationLocalizerProvider = (type, factory) =>
              {
                  return factory.Create(typeof(JsonStringLocalizer));
              }
             );

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var SupportedCulter = new CultureInfo[]
                {
                   new CultureInfo("en-US"),
                   new CultureInfo("fr-FR"),
                   new CultureInfo("ar-EG"),
                };
                //options.DefaultRequestCulture = new RequestCulture(culture: SupportedCulter[0]); // Optional: Set default culture
                options.SupportedCultures = SupportedCulter;
                //options.SupportedUICultures = SupportedCulter;

            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var rolemanger = services.GetRequiredService<RoleManager<ApplicationRole>>();

                await DefaultRoles.SeedAsync(rolemanger);
                await DefultUser.GenerateAdmin(userManager, rolemanger);
                await DefultUser.SeedAdminAllPermissions(rolemanger);


            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var supportedCulter = new[] { "en-US", "fr-FR", "ar-EG" };
            var requestLocalizationOptions = new RequestLocalizationOptions()
                //.SetDefaultCulture(supportedCulter[0]) // Optional: Set default culture
                .AddSupportedCultures(supportedCulter);

            app.UseRequestLocalization(requestLocalizationOptions);
            app.UseRequestLanguesMidelware(); //custom midelware for set culture from cookie or browser

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors(MyAllowSpecificOrigins);

            app.MapControllers();

            app.Run();
        }
    }
}
