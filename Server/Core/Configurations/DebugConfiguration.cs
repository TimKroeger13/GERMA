namespace GERMAG.Server.Core.Configurations
{
    public class DebugConfiguration : IEnviromentConfiguration
    {
        private readonly ConfigurationOptions _options;

        public DebugConfiguration(ConfigurationOptions options)
        {
            _options = options;
        }

        public string DatabaseConnection => _options.DatabaseConnection;
    }
}