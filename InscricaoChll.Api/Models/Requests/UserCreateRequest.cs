using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;

namespace InscricaoChll.Api.Models.Requests;

public class UserCreateRequest : SignUpRequest
{
    [Required(ErrorMessage = "Error_StatusIsRequired")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IEnumerable<RoleEnum> Roles { get; set; } = new List<RoleEnum>();
}