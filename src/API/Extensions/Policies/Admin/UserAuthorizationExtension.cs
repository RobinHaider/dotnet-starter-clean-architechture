namespace API.Extensions.Policies.Admin
{
    public static class UserAuthorizationExtension
    {
        public static IServiceCollection AddUserAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {

                options.AddPolicy("Admin-User-Read", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "UserPermission" && c.Value == "ReadOnly" && context.User.IsInRole("adminuser")) || context.User.IsInRole("admin") || context.User.IsInRole("superadmin")));

                options.AddPolicy("Admin-User-Add", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "UserPermission" && c.Value == "Add" && context.User.IsInRole("adminuser")) || context.User.IsInRole("admin") || context.User.IsInRole("superadmin")));

                options.AddPolicy("Admin-User-Update", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "UserPermission" && c.Value == "Update" && context.User.IsInRole("adminuser")) || context.User.IsInRole("admin") || context.User.IsInRole("superadmin")));

                options.AddPolicy("Admin-User-Delete", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "UserPermission" && c.Value == "Delete" && context.User.IsInRole("adminuser")) || context.User.IsInRole("admin") || context.User.IsInRole("superadmin")));


            });

            return services;
        }
    }
}