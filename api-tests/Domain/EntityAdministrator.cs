
using MinimalApi.Domain.Entities;

namespace ApiTests.Domain.Entities;

public class EntityAdministrator
{
    [Fact]
    public void TestGetSetProp()
    {   
        // Arrange
        var adm = new Administrator();

        // Act
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Password = "teste";
        adm.Profile = "Adm";

        // Assert
        Assert.Equal(1, adm.Id);
        Assert.Equal("teste@teste.com", adm.Email);
        Assert.Equal("teste", adm.Password);
        Assert.Equal("Adm", adm.Profile);
    }
}