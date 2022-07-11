using System.Text.Json.Serialization;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;

namespace InscricaoChll.Api.Models.Requests;

public class UserAddRoleRequest
{
    public string UserName { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IEnumerable<RoleEnum> Roles { get; set; } = new List<RoleEnum>();
}