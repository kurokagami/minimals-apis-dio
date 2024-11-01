using MinimalApi.Domain.Enuns;

namespace MinimalApi.DTOs;

public class AdministratorDTO
{
    public string Email { get; set; }  = default!;
    public string Password { get; set; }  = default!;
    public ProfileEnum? Profile { get; set; }  = default!;
}