using FluentAssertions;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
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
    }
}
