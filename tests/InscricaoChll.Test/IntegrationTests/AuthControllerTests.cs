using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using InscricaoChll.Api;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using InscricaoChll.Test.Extensions;
using InscricaoChll.Test.IntegrationTests.Config;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Xunit;

namespace InscricaoChll.Test.IntegrationTests;

[TestCaseOrderer("EcommerceApiTest.PriorityOrderer", "EcommerceApiTest")]
[Collection(nameof(IntegrationTestsFixtureCollection))]
public class AuthControllerTests
{
    private readonly IntegrationTestsFixture<StartupTests> _testsFixture;

    public AuthControllerTests(IntegrationTestsFixture<StartupTests> testsFixture)
    {
        _testsFixture = testsFixture;
    }

    #region Negative cases

    [Trait("IntegrationTest", "Controllers")]
    [Fact(DisplayName = "User sign in with invalid credentials should get error response"), TestPriority(1)]
    public async Task User_SignInWithInvalidCredentials_ShouldGerErrorResponse()
    {
        // Arrange 
        var request = new SignInRequest()
        {
            UserName = _testsFixture.EntityFixture.Faker.Internet.Email(),
            Password = "123456"
        };

        // Act 
        var response = await _testsFixture.Client.AddJsonMediaType()
            .PostAsJsonAsync("/Auth/SignIn", request);

        // Assert 
        response.IsSuccessStatusCode.Should().BeFalse();
        var responseObj = await response.DeserializeObject<BaseResponse<SignInResponse>>();
        Assert.True(string.IsNullOrEmpty(responseObj?.Data?.AccessToken));

    }

    [Trait("IntegrationTest", "Controllers")]
    [Fact(DisplayName = "Only active users can sign in")]
    public async Task SignIn_NotActiveUser_ShouldGetErrorResponse()
    {
        // Arrange 
        var user = await _testsFixture.GetUserAsync();
        var userManager = (UserManager<UserEntity>)_testsFixture.Services.GetService(typeof(UserManager<UserEntity>));

        user.Status = UserStatusEnum.Blocked;
        await userManager.UpdateAsync(user);

        var request = new SignInRequest()
        {
            UserName = _testsFixture.UserName,
            Password = _testsFixture.UserPassword
        };

        // Act 
        var response = await _testsFixture.Client.AddJsonMediaType()
            .PostAsJsonAsync("/Auth/SignIn", request);

        // Assert 
        response.StatusCode.Should().NotHaveFlag(HttpStatusCode.OK);
        var responseObj = await response.DeserializeObject<BaseResponse<SignInResponse>>();
        Assert.True(string.IsNullOrEmpty(responseObj?.Data?.AccessToken));
    }

    #endregion

    #region Positive cases

    [Trait("IntegrationTest", "Controllers")]
    [Fact(DisplayName = "User sign in with valid credentials should get success response"), TestPriority(4)]
    public async Task User_SignInWithValidCredentials_MustGetAnAcessToken()
    {
        // Arrange
        await _testsFixture.GetUserAsync();

        var request = new SignInRequest()
        {
            UserName = _testsFixture.UserName,
            Password = _testsFixture.UserPassword
        };

        // Act 
        var response = await _testsFixture.Client.AddJsonMediaType()
            .PostAsJsonAsync("/Auth/SignIn", request);

        // Assert 
        response.EnsureSuccessStatusCode();
        var responseObj =
            JsonConvert.DeserializeObject<BaseResponse<SignInResponse>>(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrEmpty(responseObj?.Data?.AccessToken));
    }

    #endregion
}