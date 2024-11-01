using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;
public interface IAdministrator
{
    Administrator Login(LoginDTO loginDTO);

    Administrator Include(Administrator adm);
    Administrator FindForID(int id);
    List<Administrator> All(int? page);
}
