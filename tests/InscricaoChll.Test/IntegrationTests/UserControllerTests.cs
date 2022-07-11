using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using InscricaoChll.Api;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using InscricaoChll.Test.Extensions;
using InscricaoChll.Test.IntegrationTests.Config;
using Xunit;

namespace InscricaoChll.Test.IntegrationTests;

[Collection(nameof(IntegrationTestsFixtureCollection))]
public class UserControllerTests
{
    private readonly IntegrationTestsFixture<StartupTests> _testsFixture;

    public UserControllerTests(IntegrationTestsFixture<StartupTests> testsFixture)
    {
        _testsFixture = testsFixture;
    }


    #region Negative Cases

    [Trait("IntegrationTest", "Controllers")]
    [Theory(DisplayName = "On create new user with invalid credentials should get error repsonse"), TestPriority(2)]
    [InlineData("emailnotvalid.com", "Test Invalid Email", "Abc123$%")]
    [InlineData("randonValidEmail", "Test Invalid Password", "123456")]
    public async Task User_TryToCreatedWithInvalidCredentials_ShouldGetErrorResponse(string email, string name, string password)
    {
        // Arrange 
        email = email != "randonValidEmail" ? email : _testsFixture.EntityFixture.Faker.Person.Email;
        var request = new SignUpRequest()
        {
            Email = email,
            Name = name,
            UserName = email,
            Password = password
        };

        if (string.IsNullOrEmpty(_testsFixture.AdminAccessToken))
            await _testsFixture.AuthenticateAsAdminAsync();

        // Act 
        var response = await _testsFixture.Client.AddToken(_testsFixture.AdminAccessToken)
            .PostAsJsonAsync("/User", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        var responseObj = await response.DeserializeObject<BaseResponse>();
        responseObj.Errors.Should().HaveCountGreaterThan(0, email != "randonValidEmail" ? "Invalid password." : "Invalid email.");
    }

    [Trait("IntegrationTest", "Controllers")]
    [Fact(DisplayName = "Cannot change own loged user status")]
    public async Task Status_CannotChangeOwnStatus_ShouldGetErrorResponse()
    {
        // Arrange 
        var userService = (IUserService)_testsFixture.Services.GetService(typeof(IUserService));
        var userResponse = await userService.FindByUserName(_testsFixture.AdminUserName);
        var request = new UserChangeStatusRequest()
        {
            UserId = userResponse.Data.Id,
            Status = UserStatusEnum.Active
        };

        if (string.IsNullOrEmpty(_testsFixture.AdminAccessToken))
            await _testsFixture.AuthenticateAsAdminAsync();

        // Act 
        var response = await _testsFixture.Client.AddToken(_testsFixture.AdminAccessToken)
            .PatchAsJsonAsync("/User/Status", request);

        // Assert 
        response.StatusCode.Should().NotHaveFlag(HttpStatusCode.OK);
        var responseObj = await response.DeserializeObject<BaseResponse>();
        responseObj.Success.Should().BeFalse();
        Assert.True(responseObj.Errors.Any(e => e.ErrorCode == "UserCannotChangeOwnStatus"));
    }

    #endregion

    #region Positive Cases

    [Trait("IntegrationTest", "Controllers")]
    [Fact(DisplayName = "Should get list of users")]
    public async Task Search_AllUsers_ShouldGetListOfUsers()
    {
        // Arrange 
        if (string.IsNullOrEmpty(_testsFixture.AdminAccessToken))
            await _testsFixture.AuthenticateAsAdminAsync();

        // Act
        var response = await _testsFixture.Client.AddToken(_testsFixture.AdminAccessToken)
            .GetAsync("/User/Search");

        // Assert 
        response.EnsureSuccessStatusCode();
        var responseObj = await response.DeserializeObject<PagedResponse<UserModel>>();
        responseObj.Success.Should().BeTrue();
        responseObj.Errors.Should().HaveCount(0);
        responseObj.Data.Should().NotBeNull();
        responseObj.Data.Should().HaveCountGreaterThan(0);
    }

    [Trait("IntegrationTest", "Controllers")]
    [Fact(DisplayName = "On create new user with valid credentials should get success repsonse"), TestPriority(3)]
    public async Task User_TryToCreatedWithValidCredentials_MustOccourSuccessfully()
    {
        // Arrange 
        _testsFixture.GenerateUserAndPassword();
        var request = new SignUpRequest()
        {
            Email = _testsFixture.UserName,
            Name = _testsFixture.EntityFixture.Faker.Person.FullName,
            UserName = _testsFixture.UserName,
            Password = _testsFixture.UserPassword
        };
     
        // Act 
        var response = await _testsFixture.Client
            .PostAsJsonAsync("/User", request);

        // Assert 
        response.EnsureSuccessStatusCode();
        var responseObj = await response.DeserializeObject<BaseResponse>();
        responseObj.Success.Should().BeTrue();
        responseObj.Errors.Should().HaveCount(0);
    }

    [Trait("IntegrationTest", "Controllers")]
    [Theory(DisplayName = "Change other user's status, should update it successfuly")]
    [InlineData(RoleEnum.Admin, UserStatusEnum.Blocked)]
    [InlineData(RoleEnum.User, UserStatusEnum.Blocked)]
    public async Task ChangeStatus_GivenAllowedUser_ShouldUpdateSuccessfuly(RoleEnum role, UserStatusEnum status)
    {
        // Arrange 
        var user = await _testsFixture.GetUserAsync();

        var request = new UserChangeStatusRequest()
        {
            UserId = user.Id,
            Status = status
        };

        if (string.IsNullOrEmpty(_testsFixture.AdminAccessToken))
            await _testsFixture.AuthenticateAsAdminAsync();

        // Act 
        var response = await _testsFixture.Client.AddToken(_testsFixture.AdminAccessToken)
            .PatchAsJsonAsync("/User/Status", request);

        // Assert 
        response.EnsureSuccessStatusCode();
        var responseObj = await response.DeserializeObject<BaseResponse>();
        responseObj.Success.Should().BeTrue();
        responseObj.Errors.Should().HaveCount(0);
    }

    #endregion
}