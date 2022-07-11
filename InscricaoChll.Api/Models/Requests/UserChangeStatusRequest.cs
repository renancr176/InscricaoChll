using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;

namespace InscricaoChll.Api.Models.Requests;

public class UserChangeStatusRequest
{
    [Required(ErrorMessage = "Usuário inválido")]
    public Guid UserId { get; set; }
    [Required(ErrorMessage = "Status inválido")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserStatusEnum Status { get; set; }
}