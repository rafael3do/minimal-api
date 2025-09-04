using MinimalApi.Domain.Entity;
using MinimalApi.Domain.Interfaces;
using MinimalApi.DTOs;

namespace Test.Mock;

public class AdministratorServiceMock : IAdministratorService
{
    private static List<Administrator> administrators = new List<Administrator>(){
        new Administrator{
            Id = 1,
            Email = "adm@teste.com",
            Senha = "123456",
            Perfil = "Adm"
        },
        new Administrator{
            Id = 2,
            Email = "editor@teste.com",
            Senha = "123456",
            Perfil = "Editor"
        }
    };
    
    public Administrator? BuscaPorId(int id)
    {
        return administrators.Find(a => a.Id == id);
    }

    public Administrator Incluir(Administrator administrator)
    {
        administrator.Id = administrators.Count() + 1;
        administrators.Add(administrator);

        return administrator;
    }

    public Administrator? Login(LoginDTO loginDTO)
    {
        return administrators.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
    }

    public List<Administrator> All(int? pagina)
    {
        return administrators;
    }
}