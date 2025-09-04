using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.Service;
using MinimalApi.Infrastructure.Db;

namespace Test.Domain.Service;

[TestClass]
public class AdministratorServiceTest
{
    private DatabaseContext CriarContextoDeTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        //Configuração do ConfigurationBuilder
        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DatabaseContext(configuration);
    }

    [TestMethod]
    public void TestarSalvarAdministrador()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administrators");

        var adm = new Administrator();
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        var administratorService = new AdministratorService(context);

        // Act
        administratorService.Incluir(adm);
      
        // Assert
        Assert.AreEqual(1, administratorService.All(1).Count());
    }

    public void TestarBuscaPorIdAdministrador()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administrators");

        var adm = new Administrator();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        var administratorService = new AdministratorService(context);

        // Act
        administratorService.Incluir(adm);
        var admDoBanco = administratorService.BuscaPorId(adm.Id);
        
        // Assert
        Assert.AreEqual(1, admDoBanco?.Id);
    }
}