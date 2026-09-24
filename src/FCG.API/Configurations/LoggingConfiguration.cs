namespace FCG.API.Configurations;

public static class LoggingConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddLoggingConfiguration()
        {
            builder.Logging.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
            });
        }
    }
}