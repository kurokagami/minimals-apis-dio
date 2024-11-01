using Microsoft.AspNetCore.Mvc.Testing;
using MinimalApi.Domain.Interfaces;
using ApiTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace ApiTests.Helpers
{
    public class Setup : WebApplicationFactory<Startup>
    {
        public HttpClient Client { get; private set; }

        public Setup()
        {
            Client = CreateClient(); // Inicializa o cliente HTTP
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Substitui o serviço IAdministrator pelo mock
                services.AddScoped<IAdministrator, AdministratorServiceMock>();
            });
        }
    }
}
