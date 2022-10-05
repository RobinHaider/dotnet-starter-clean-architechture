using System.Text;
using API.Helpers;
using API.Services;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            IdentityBuilder builder = services.AddIdentityCore<AppUser>(opt =>
           {
               opt.Password.RequireDigit = false;
               opt.Password.RequiredLength = 4;
               opt.Password.RequireNonAlphanumeric = false;
               opt.Password.RequireUppercase = false;
               opt.Password.RequireLowercase = false;

               opt.User.RequireUniqueEmail = true;
               opt.SignIn.RequireConfirmedEmail = false;
               opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
               opt.Lockout.AllowedForNewUsers = true;
               opt.Lockout.MaxFailedAccessAttempts = 10;
               opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

           }).AddTokenProvider<EmailConfirmationTokenProvider<AppUser>>("emailconfirmation").AddDefaultTokenProviders();
            services.Configure<EmailConfirmationTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromDays(1));
            services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromDays(1));
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<DataContext>();
            builder.AddRoleValidator<RoleValidator<IdentityRole>>();
            builder.AddRoleManager<RoleManager<IdentityRole>>();
            builder.AddSignInManager<SignInManager<AppUser>>();


            // add jwt authentication sceme
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:Key"])),
                        ValidIssuer = configuration["Token:Issuer"],
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true, // validate expire date
                        // it will unvalidate imdealty after token expire, which deafult was five minute after expires
                        ClockSkew = TimeSpan.Zero
                    };
                    // for signalR
                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                       {
                           var accessToken = context.Request.Query["access_token"];

                           var path = context.HttpContext.Request.Path;
                           if (!string.IsNullOrEmpty(accessToken) &&
                               path.StartsWithSegments("/hubs"))
                           {
                               context.Token = accessToken;
                           }

                           return Task.CompletedTask;
                       }
                    };
                });

            // for custom authrization policy..
            services.AddAuthorization(opt =>
           {
               opt.AddPolicy("IsActivityHost", policy =>
               {
                   policy.Requirements.Add(new IsHostRequirement());
               });
           });
            services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
            // ---------------

            services.AddScoped<TokenService>();

            return services;
        }
    }
}