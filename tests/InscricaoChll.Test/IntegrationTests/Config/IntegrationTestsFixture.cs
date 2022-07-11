using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using InscricaoChll.Api;
using InscricaoChll.Api.DbContexts.ChllDbContext;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Models;
using InscricaoChll.Api.Models.Requests;
using InscricaoChll.Api.Models.Responses;
using InscricaoChll.Test.Extensions;
using InscricaoChll.Test.Fixtures;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Xunit;

namespace InscricaoChll.Test.IntegrationTests.Config;

[CollectionDefinition(nameof(IntegrationTestsFixtureCollection))]
public class IntegrationTestsFixtureCollection : ICollectionFixture<IntegrationTestsFixture<StartupTests>> { }

public class IntegrationTestsFixture<TStartup> : IDisposable where TStartup : class
{
    public readonly StartupFactory<TStartup> Factory;
    public HttpClient Client;
    public EntityFixture EntityFixture;
    public IServiceProvider Services;
    public ChllDbContext ChllDb;

    public string AdminUserName { get; set; }
    public string AdminPassword { get; set; }
    public string AdminAccessToken { get; set; }

    public string UserName { get; set; }
    public string UserPassword { get; set; }
    public string UserAccessToken { get; set; }

    public IntegrationTestsFixture()
    {
        Factory = new StartupFactory<TStartup>();
        Client = Factory.CreateClient();
        EntityFixture = new EntityFixture();

        AdminUserName = "usertest@gmail.com";
        AdminPassword = "5XWkgWPLD)pHAJ=3wE):<2E=Hwr{.~54";

        Services = Factory.Server.Services;
        ChllDb = (ChllDbContext)Services.GetService(typeof(ChllDbContext));

        Task.Run(async () =>
        {
            var userService = (IUserService)Services.GetService(typeof(IUserService));
            var userManager = (UserManager<UserEntity>)Services.GetService(typeof(UserManager<UserEntity>));
            if (userService != null)
            {
                var user = await userService.FindByUserName(AdminUserName);
                if (user.Data == null)
                {
                    user = await userService.SignUpAsync(new SignUpRequest()
                    {
                        Email = AdminUserName,
                        UserName = AdminUserName,
                        Password = AdminPassword,
                        Name = "Admin Test User",
                    }, new List<RoleEnum>()
                        {
                            RoleEnum.Admin
                        });
                }

                var updateUser = await userManager.FindByIdAsync(user.Data.Id.ToString());
                updateUser.EmailConfirmed = true;
                await userManager.UpdateAsync(updateUser);
            }

            await ChllDb.SaveChangesAsync();
        }).Wait();
    }

    public void GenerateUserAndPassword()
    {
        var faker = new Faker("pt_BR");
        UserName = faker.Internet.Email().ToLower();
        UserPassword = faker.Internet.Password(8, false, "", "Ab@1_");
    }

    public async Task AuthenticateAsAdminAsync()
    {
        AdminAccessToken = await AuthenticateAsync(AdminUserName, AdminPassword);
    }

    public async Task AuthenticateAsUserAsync()
    {
        UserAccessToken = await AuthenticateAsync(UserName, UserPassword);
    }

    public async Task<string> AuthenticateAsync(string userName, string password)
    {
        var request = new SignInRequest
        {
            UserName = userName,
            Password = password
        };

        // Recriando o client para evitar configurações de outro startup.
        Client = Factory.CreateClient();

        var response = await Client.AddJsonMediaType()
            .PostAsJsonAsync("/Auth/SignIn", request);
        response.EnsureSuccessStatusCode();
        var responseObj =
            JsonConvert.DeserializeObject<BaseResponse<SignInResponse>>(await response.Content.ReadAsStringAsync());
        if (string.IsNullOrEmpty(responseObj?.Data.AccessToken))
            throw new ArgumentNullException("AccessToken", "Unable to retrieve authentication token.");

        return responseObj?.Data.AccessToken;
    }

    public async Task<UserEntity> GetUserAsync()
    {
        var userService = (IUserService)Services.GetService(typeof(IUserService));
        var userManager = (UserManager<UserEntity>)Services.GetService(typeof(UserManager<UserEntity>));

        BaseResponse<UserModel> result;
        var retries = 3;
        do
        {
            var firstName = EntityFixture.Faker.Person.FirstName;
            var lastName = EntityFixture.Faker.Person.LastName;

            UserName = EntityFixture.Faker.Internet.Email(firstName, lastName);
            UserPassword = EntityFixture.Faker.Internet.Password(8, prefix: "Ab@1");

            result = await userService.SignUpAsync(new SignUpRequest()
            {
                Name = $"{firstName} {lastName}",
                Email = UserName,
                UserName = UserName,
                Password = UserPassword
            }, new[] { RoleEnum.User });

            if (!result.Success)
            {
                retries--;
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        } while (!result.Success && retries > 0);

        var updateUser = await userManager.FindByIdAsync(result.Data.Id.ToString());
        updateUser.EmailConfirmed = true;
        await userManager.UpdateAsync(updateUser);
       
        return updateUser;
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}