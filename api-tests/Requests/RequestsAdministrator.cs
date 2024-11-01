using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ApiTests.Helpers;
using MinimalApi.Domain.ModelViews;
using MinimalApi.DTOs;

namespace ApiTests.Requests
{
    public class RequestsAdministrator : IClassFixture<Setup>
    {
        private readonly HttpClient _client;

        public RequestsAdministrator(Setup setup)
        {
            _client = setup.Client; // Acesse o HttpClient configurado na fixture
        }

        [Fact]
        public async Task TestarLogin()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@test.com",
                Password = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/administrators/login", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            var loggedAdmin = JsonSerializer.Deserialize<AdministratorLogged>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Corrigido aqui
            });

            Assert.NotNull(loggedAdmin);
            Assert.NotNull(loggedAdmin.Email);
            Assert.NotNull(loggedAdmin.Token);
            Assert.NotNull(loggedAdmin.Profile);
            Assert.Equal(loginDTO.Email, loggedAdmin.Email);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Verifica se o status é 200 OK
        }
    }
}
