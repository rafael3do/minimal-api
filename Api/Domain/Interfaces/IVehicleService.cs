using MinimalApi.Domain.Entity;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IVehicleService
{
    List<Vehicle> All(int? pagina = 1, string? nome = null, string? marca = null);
    Vehicle? BuscaPorId(int id);
    void Incluir(Vehicle vehicle);
    void Atualizar(Vehicle vehicle);
    void Apagar(Vehicle vehicle);
}