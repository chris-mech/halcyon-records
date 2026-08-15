using System.Text;
using HalcyonRecords.Api.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HalcyonRecords.Api.Infrastructure.Auth;

public static class AuthExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiAuth(IConfiguration configuration)
        {
            var jwtOptions =
                configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                ?? throw new InvalidOperationException("Jwt configuration section is missing.");

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.AddSingleton<JwtTokenService>();

            var passwordPolicy =
                configuration
                    .GetSection(PasswordPolicyOptions.SectionName)
                    .Get<PasswordPolicyOptions>()
                ?? new PasswordPolicyOptions();

            services
                .AddIdentityCore<User>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = passwordPolicy.RequiredLength;
                    options.Password.RequiredUniqueChars = passwordPolicy.RequiredUniqueChars;
                    options.Password.RequireNonAlphanumeric = passwordPolicy.RequireNonAlphanumeric;
                    options.Password.RequireLowercase = passwordPolicy.RequireLowercase;
                    options.Password.RequireUppercase = passwordPolicy.RequireUppercase;
                    options.Password.RequireDigit = passwordPolicy.RequireDigit;
                })
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(bearerOptions =>
                {
                    bearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
                        ),
                    };
                });

            services.AddAuthorizationBuilder();

            return services;
        }
    }
}
