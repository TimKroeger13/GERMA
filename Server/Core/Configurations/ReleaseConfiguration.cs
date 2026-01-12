namespace GERMAG.Server.Core.Configurations
{
    public class ReleaseConfiguration : IEnviromentConfiguration
    {
        private readonly ConfigurationOptions _options;

        public ReleaseConfiguration(ConfigurationOptions options)
        {
            _options = options;
        }

        public string DatabaseConnection => Environment.GetEnvironmentVariable(_options.DatabaseConnection) ??
            throw new Exception("Databaseconnection not found");
    }

}
