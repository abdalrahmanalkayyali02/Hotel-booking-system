using FluentValidation;
using HBS.API.Db.Repo.Interface;
using HBS.API.Db.Repo.Impl;
using HBS.API.Services.Interface;
using HBS.API.Services.Impl;
using HBS.API.Validation;
using HBS.API.Dtos.Users.Registeration;
using HBS.API.Dtos.Users.Update;
using HBS.API.Settings;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using HBS.API.Db.Interceptors;
using HBS.API.Db.UnitOfWork.Impl;
using HBS.API.Db.UnitOfWork.Interface;
using HBS.API.Dtos.Authentication.ChangePassword;
using HBS.API.Dtos.Authentication.ForgotPassword;
using HBS.API.Dtos.Hotels.CreateHotel;
using HBS.API.Dtos.Hotels.UpdateHotel;
using HBS.API.integrations.Interface;
using HBS.API.integrations.Provider;
using CloudinaryDotNet;

namespace HBS.API.DI;

public static class Di
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        //repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IRolePermissionsRepository, RolePermissionsRepository>();
        services.AddScoped<IRegionsRepository, RegionsRepository>();
        services.AddScoped<ISubRegionsRepository, SubRegionsRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<ILanguagesRepository, LanguagesRepository>();
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        services.AddScoped<ICitiesRepository, CitiesRepository>();
        services.AddScoped<IHotelsRepository, HotelRepository>();
        services.AddScoped<IHotelRequestRepository, HotelRequestRepository>();
        services.AddScoped<IHotelsTranslationRepository, HotelsTranslationRepository>();
        services.AddScoped<IHotelImagesRepository, HotelImagesRepository>();

        //Configurations
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<TokenSettings>(configuration.GetSection("TokenSettings"));
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

        var cloudinarySettings = configuration
                                   .GetSection("CloudinarySettings")
                                   .Get<CloudinarySettings>()
                                 ?? throw new InvalidOperationException(
                                   "Cloudinary settings configuration is missing"
                                 );

        var cloudinaryAccount = new Account(
          cloudinarySettings.CloudName,
          cloudinarySettings.ApiKey,
          cloudinarySettings.ApiSecret
        );

        var cloudinary = new Cloudinary(cloudinaryAccount);

        cloudinary.Api.Secure = true;

        services.AddSingleton(cloudinary);

        //Redis
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is missing");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnectionString)
        );

        //services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IImageService, ImageService>();

        //validators
        services.AddScoped<IValidator<RegisterStandardUsersDtos>, UserValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordValidator>();
        services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordValidator>();
        services.AddScoped<IValidator<CreateHotelRequest>, CreateHotelValidator>();
        services.AddScoped<IValidator<UpdateHotelRequest>, UpdateHotelValidator>();

        //Interceptors
        services.AddHttpContextAccessor();
        services.AddScoped<AuditSaveChangesInterceptor>();

        //UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Token Authentication
        var tokenSettings = configuration
            .GetSection("TokenSettings")
            .Get<TokenSettings>()
            ?? throw new InvalidOperationException("Token settings configuration is missing");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = tokenSettings.Issuer,
                    ValidAudience = tokenSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(tokenSettings.SecretKey)
                    )
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jti = context.Principal?
                            .FindFirst(JwtRegisteredClaimNames.Jti)?
                            .Value;

                        if (string.IsNullOrEmpty(jti))
                        {
                            context.Fail("Token does not contain a jti.");
                            return;
                        }

                        var tokenService =
                            context.HttpContext.RequestServices
                                .GetRequiredService<ITokenService>();

                        var isBlacklisted =
                            await tokenService.IsTokenBlacklistedAsync(jti);

                        if (isBlacklisted)
                        {
                            context.Fail("Token has been revoked.");
                        }
                    }
                };
            });

        //Authorization
        services.AddAuthorization();

        return services;
    }
}
