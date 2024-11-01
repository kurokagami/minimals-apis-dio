
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Services;
using MinimalApi.Infrasctrcuture.Db;

namespace ApiTests.Domain.Services;

public class ServiceAdministrator
{   
private static DataBaseContext CreateContextTests()
{
    // Obtém o diretório do assembly atual
    var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    
    // Combina o caminho do assembly com o nome do arquivo de configuração
    var configFilePath = Path.Combine(assemblyPath ?? Directory.GetCurrentDirectory(), "appsettings.json");

    // Verifica se o arquivo existe
    if (!File.Exists(configFilePath))
    {
        throw new FileNotFoundException($"O arquivo de configuração 'appsettings.json' não foi encontrado em: {configFilePath}");
    }

    // Configura o ConfigurationBuilder
    var builder = new ConfigurationBuilder()
        .SetBasePath(assemblyPath) // Define o diretório base corretamente
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    var configuration = builder.Build();

    return new DataBaseContext(configuration);
}


    [Fact]
    public void TestSaveAdministrator()
    {   
        // Arrange
        var context = CreateContextTests();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administrators"); //Limpa tabela

        var adm = new Administrator();
        adm.Email = "teste@teste.com";
        adm.Password = "teste";
        adm.Profile = "Adm";

        var admService = new AdministratorService(context);

        // Act
        admService.Include(adm);
        var admBase = admService.FindForID(adm.Id);

        // Assert
     
        Assert.Equal(1, admBase?.Id);
    }
}