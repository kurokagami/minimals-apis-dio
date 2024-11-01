using MinimalApi.Domain.Entities;
using MinimalApi.Domain.Interfaces;
using MinimalApi.DTOs;

namespace ApiTests.Mocks
{
    public class AdministratorServiceMock : IAdministrator
    {
        private static List<Administrator> adms = new List<Administrator>
        {
            new Administrator { Id = 1, Email = "adm@test.com", Password = "123456", Profile = "Adm" },
            new Administrator { Id = 2, Email = "editor@test.com", Password = "123456", Profile = "Editor" }
        };

        public List<Administrator> All(int? page)
        {
            return adms;
        }

        public Administrator FindForID(int id)
        {
            return adms.Find(a => a.Id == id);
        }

        public Administrator Include(Administrator adm)
        {
            adm.Id = adms.Count + 1;
            adms.Add(adm);
            return adm;
        }

        public Administrator Login(LoginDTO loginDTO)
        {
            return adms.Find(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password);
        }

    }
}
