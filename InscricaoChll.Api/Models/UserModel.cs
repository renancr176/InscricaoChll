using System.Text.Json.Serialization;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;

namespace InscricaoChll.Api.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IEnumerable<RoleEnum> Roles { get; set; } = new List<RoleEnum>();
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserStatusEnum Status { get; set; }
}