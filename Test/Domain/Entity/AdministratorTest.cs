using MinimalApi.Domain.Entity;

namespace Test.Domain.Entity;

[TestClass]
public class AdministratorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var adm = new Administrator();

        // Act
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";

        // Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }
}