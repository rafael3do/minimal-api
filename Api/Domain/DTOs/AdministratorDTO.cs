using MinimalApi.Domain.Enum;

namespace MinimalApi.DTOs;
public class AdministratorDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public Perfil? Perfil { get; set; } = default!;
}