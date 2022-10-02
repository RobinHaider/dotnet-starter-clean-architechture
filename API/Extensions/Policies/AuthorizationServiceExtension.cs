using API.Extensions.Policies.Admin;

namespace API.Extensions.Policies
{
    public static class AuthorizationServiceExtension
    {
        public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {

                options.AddPolicy("AdminAccess", policy => policy.RequireRole("admin"));
                options.AddPolicy("SuperAdminAccess", policy => policy.RequireRole("superadmin"));
            });


            services.AddUserAuthorization();

            return services;
        }
    }
}