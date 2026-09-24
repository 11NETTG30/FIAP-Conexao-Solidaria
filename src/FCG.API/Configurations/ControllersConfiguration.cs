using System.Text.Json.Serialization;

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
                })
                // Enums trafegam pelo nome (ex.: "Cancelada") — números continuam aceitos
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        }
    }
}