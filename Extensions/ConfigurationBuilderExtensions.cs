using Microsoft.Extensions.Configuration;

namespace ITECHAutoAttendance.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonFileFromDockerVolumeIfExistent(this IConfigurationBuilder builder, string path) => 
        Environment.GetEnvironmentVariable("DOCKER") == "Enabled" ? builder.AddJsonFile(path) : builder;
}