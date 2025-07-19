using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApi.Database.Models;
using WebApi.Tests.Extensions;
using WebApi.Tests.Setup;

using WebApi.Tests.UserControllerTests.TestData;
using Xunit;

namespace WebApi.Tests.UserControllerTests
{
    [Collection("DatabaseCollection")]
    public class UserControllerIntegration : DatabaseIntegrationBase
    {
        private readonly HttpClient _client;

        public UserControllerIntegration(DataSetupFixture dbFixture, TestWebApplicationFactory factory)
            : base(dbFixture, factory)
        {
            _client = factory.CreateClient();
        }

        [Fact(DisplayName = "Me endpoint returns 200 with valid token")]
        [Trait("Integration Test", "Happy Path")]
        public async Task MeEndpoint_WithValidToken_ReturnsUserName()
        {
            var dataSetup = new UserNonAdminData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001"; // From MeTestDataSetup.Seed
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );
            var response = await _client.GetAsync("api/user/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("test_user"); // Matches UserName from Seed

            dataSetup.Clean();
        }

        [Fact(DisplayName = "Me endpoint returns 401 without token")]
        [Trait("Integration Test", "Authentication")]
        public async Task MeEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("api/user/me");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "Me endpoint returns 401 with invalid claims")]
        [Trait("Integration Test", "Authentication")]
        public async Task MeEndpoint_WithInvalidClaims_ReturnsUnauthorized()
        {
            _client.SetFakeJwtToken(new Claim("iss", "test-issuer"));
            var response = await _client.GetAsync("api/user/me");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "Create user returns 201 with valid admin token")]
        [Trait("Integration Test", "Happy Path")]
        public async Task CreateUser_WithAdminToken_ReturnsCreated()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var newUser = new UserDBO
            {
                UserId = "newuser001",
                UserName = "new_user",
                IsAdmin = false,
                IsRoot = false,
                IsLockedOut = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(newUser),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("api/user", content);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("new_user");

            var locationHeader = response.Headers.Location?.ToString();
            locationHeader.Should().ContainEquivalentOf("api/user/newuser001");

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Create user returns 403 for non-admin user")]
        [Trait("Integration Test", "Authorization")]
        public async Task CreateUser_NonAdmin_ReturnsForbidden()
        {
            var dataSetup = new UserNonAdminData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001";
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );

            var newUser = new UserDBO
            {
                UserId = "newuser002",
                UserName = "new_user2",
                IsAdmin = false,
                IsRoot = false,
                IsLockedOut = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(newUser),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("api/user", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            dataSetup.Clean();
        }

        [Fact(DisplayName = "Create user returns 409 when user already exists")]
        [Trait("Integration Test", "Error Case")]
        public async Task CreateUser_ExistingUser_ReturnsConflict()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var existingUser = new UserDBO
            {
                UserId = "admin001", // Already exists in AdminUserControllerData
                UserName = "duplicate_user",
                IsAdmin = false,
                IsRoot = false,
                IsLockedOut = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(existingUser),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("api/user", content);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Update user returns 204 with valid admin token")]
        [Trait("Integration Test", "Happy Path")]
        public async Task UpdateUser_WithAdminToken_ReturnsNoContent()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "user001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var updatedUser = new UserDBO
            {
                UserId = targetUserId,
                UserName = "updated_user",
                IsAdmin = true,
                IsRoot = false, // Should be ignored due to preservation in controller
                IsLockedOut = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updatedUser),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PutAsync($"api/user/{targetUserId}", content);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update in the database
            var db = new QueryFactory(Connection, new SqlServerCompiler());
            var dbUser = db.Query("Users").Where("UserId", targetUserId).FirstOrDefault<UserDBO>();
            dbUser.UserName.Should().Be("updated_user");
            dbUser.IsAdmin.Should().Be(true);
            dbUser.IsLockedOut.Should().Be(true);
            dbUser.IsRoot.Should().Be(false); // Should remain unchanged

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Update user returns 403 for non-admin user")]
        [Trait("Integration Test", "Authorization")]
        public async Task UpdateUser_NonAdmin_ReturnsForbidden()
        {
            var dataSetup = new UserNonAdminData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001";
            const string targetUserId = "test001";
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );

            var updatedUser = new UserDBO
            {
                UserId = targetUserId,
                UserName = "updated_user",
                IsAdmin = true,
                IsRoot = false,
                IsLockedOut = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updatedUser),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PutAsync($"api/user/{targetUserId}", content);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            dataSetup.Clean();
        }

        [Fact(DisplayName = "Update user returns 404 for non-existent user")]
        [Trait("Integration Test", "Error Case")]
        public async Task UpdateUser_NonExistentUser_ReturnsNotFound()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "nonexistent001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var updatedUser = new UserDBO
            {
                UserId = targetUserId,
                UserName = "updated_user",
                IsAdmin = true,
                IsRoot = false,
                IsLockedOut = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updatedUser),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PutAsync($"api/user/{targetUserId}", content);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Delete user returns 204 with valid admin token")]
        [Trait("Integration Test", "Happy Path")]
        public async Task DeleteUser_WithAdminToken_ReturnsNoContent()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "user001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.DeleteAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the user was deleted
            var db = new QueryFactory(Connection, new SqlServerCompiler());
            var dbUser = db.Query("Users").Where("UserId", targetUserId).FirstOrDefault<UserDBO>();
            dbUser.Should().BeNull();

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Delete user returns 403 for non-admin user")]
        [Trait("Integration Test", "Authorization")]
        public async Task DeleteUser_NonAdmin_ReturnsForbidden()
        {
            var dataSetup = new UserNonAdminData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001";
            const string targetUserId = "test001";
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.DeleteAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            dataSetup.Clean();
        }

        [Fact(DisplayName = "Delete user returns 404 for non-existent user")]
        [Trait("Integration Test", "Error Case")]
        public async Task DeleteUser_NonExistentUser_ReturnsNotFound()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "nonexistent001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.DeleteAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get user by ID with admin token returns 200 and user data")]
        [Trait("Integration Test", "Happy Path")]
        public async Task GetUserById_AdminAccess_ReturnsUserData()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "user001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDTO>(responseContent);
            user.Should().NotBeNull();
            user.UserId.Should().Be(targetUserId);
            user.UserName.Should().Be("regular_user"); // From AdminUserData seed

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get user by ID with non-admin token returns 403")]
        [Trait("Integration Test", "Authorization")]
        public async Task GetUserById_NonAdminAccess_ReturnsForbidden()
        {
            var dataSetup = new UserNonAdminData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001";
            const string targetUserId = "test001";
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            dataSetup.Clean();
        }

        [Fact(DisplayName = "Get user by ID for non-existent user returns 404")]
        [Trait("Integration Test", "Error Case")]
        public async Task GetUserById_NonExistentId_ReturnsNotFound()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "nonexistent001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get user by ID without token returns 401")]
        [Trait("Integration Test", "Authentication")]
        public async Task GetUserById_WithoutToken_ReturnsUnauthorized()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string targetUserId = "user001";
            var response = await _client.GetAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get user by ID with invalid claims returns 401")]
        [Trait("Integration Test", "Authentication")]
        public async Task GetUserById_InvalidClaims_ReturnsUnauthorized()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string targetUserId = "user001";
            _client.SetFakeJwtToken(new Claim("iss", "test-issuer"));

            var response = await _client.GetAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get user by ID verifies database consistency")]
        [Trait("Integration Test", "Happy Path")]
        public async Task GetUserById_AdminAccess_VerifiesDatabase()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            const string targetUserId = "user001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync($"api/user/{targetUserId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var db = new QueryFactory(Connection, new SqlServerCompiler());
            var dbUser = await db.Query("Users").Where("UserId", targetUserId).FirstOrDefaultAsync<UserDBO>();
            dbUser.Should().NotBeNull();
            dbUser.UserId.Should().Be(targetUserId);
            dbUser.UserName.Should().Be("regular_user");

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get all users with admin token returns 200 and paginated users")]
        [Trait("Integration Test", "Happy Path")]
        public async Task GetAllUsers_AdminAccess_ReturnsPaginatedUsers()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync("api/user?page=1&pageSize=2");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDTO>>(responseContent);
            users.Should().NotBeNull();
            users.Count.Should().BeLessThanOrEqualTo(2); // pageSize=2
           
            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get all users with non-admin token returns 403")]
        [Trait("Integration Test", "Authorization")]
        public async Task GetAllUsers_NonAdminAccess_ReturnsForbidden()
        {
            var dataSetup = new UserNonAdminData(Connection);
            AddTestDataSetup(dataSetup);

            const string userId = "test001";
            _client.SetFakeJwtToken(
                new Claim("sub", userId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync("api/user");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            dataSetup.Clean();
        }

        [Fact(DisplayName = "Get all users with invalid pagination parameters returns 400")]
        [Trait("Integration Test", "Error Case")]
        public async Task GetAllUsers_InvalidPageParams_ReturnsBadRequest()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync("api/user?page=-1&pageSize=0");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get all users with large page size returns correct users")]
        [Trait("Integration Test", "Happy Path")]
        public async Task GetAllUsers_LargePageSize_ReturnsUsers()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync("api/user?page=1&pageSize=100");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDTO>>(responseContent);
            users.Should().NotBeNull();
            users.Should().Contain(u => u.UserId == "user001");
            users.Should().Contain(u => u.UserId == "admin001");            

            adminDataSetup.Clean();
        }

        [Fact(DisplayName = "Get all users with second page returns correct subset")]
        [Trait("Integration Test", "Happy Path")]
        public async Task GetAllUsers_SecondPage_ReturnsSubset()
        {
            var adminDataSetup = new AdminUserData(Connection);
            AddTestDataSetup(adminDataSetup);

            const string adminUserId = "admin001";
            _client.SetFakeJwtToken(
                new Claim("sub", adminUserId),
                new Claim("iss", "test-issuer")
            );

            var response = await _client.GetAsync("api/user?page=2&pageSize=1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDTO>>(responseContent);
            users.Should().NotBeNull();
            users.Count.Should().BeLessThanOrEqualTo(1); // pageSize=1
            users.Should().Contain(u => u.UserId == "admin001" || u.UserId == "user001"); // Second page, assuming ordering

            adminDataSetup.Clean();
        }
    }
}
