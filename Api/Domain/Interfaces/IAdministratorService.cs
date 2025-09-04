using MinimalApi.Domain.Entity;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IAdministratorService
{
    Administrator? Login(LoginDTO loginDTO);
    Administrator Incluir(Administrator administrator);
    Administrator? BuscaPorId(int id);
    List<Administrator> All(int? pagina);
}