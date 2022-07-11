using System.ComponentModel.DataAnnotations;

namespace InscricaoChll.Api.Models.Requests;

public class PasswordResetRequest
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string UserName { get; set; }
}