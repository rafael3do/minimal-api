using MinimalApi.Domain.Enum;

namespace MinimalApi.Domain.ModelViews;

public record AdministratorLogged
{
    public string Email { get; set; } = default!;
    public string Perfil { get; set; } = default!;
    public string Token { get; set; } = default!;
}