using System.Runtime.CompilerServices;

namespace GERMAG.Server.Core.Configurations
{
    public class ConfigurationOptions
    {
        public const string Configuration = nameof(Configuration);
        public string DatabaseConnection { get; set; } = "";
    }
}
