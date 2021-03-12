using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace AzureCognitiveSearch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    var builtConfiguration = config.Build();

                    string keyVault = builtConfiguration["KeyVault:Vault"];
                    string tenantId = builtConfiguration["KeyVault:TenantId"];
                    string clientId = builtConfiguration["KeyVault:ClientId"];
                    string clientSecret = builtConfiguration["KeyVault:ClientSecret"];

                    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

                    var client = new SecretClient(new Uri(keyVault), credential);
                    config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
