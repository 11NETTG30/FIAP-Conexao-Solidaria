namespace FCG.API.Configurations;

public static class ControllersConfiguration
{
    extension(IServiceCollection services)
    {
        public void AddControllersConfiguration()
        {
            services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            });
        }
    }
}