using System.Text.Json.Serialization;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;

namespace InscricaoChll.Api.Models.Requests;

public class UserSearchRequest : PagedRequest
{
    public IEnumerable<RoleEnum> HasRoles { get; set; } = new List<RoleEnum>();
    public IEnumerable<RoleEnum> NotHaveRoles { get; set; } = new List<RoleEnum>();
    public string? Name { get; set; }
}