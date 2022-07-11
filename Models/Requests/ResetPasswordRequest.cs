using System.ComponentModel.DataAnnotations;

namespace InscricaoChll.Api.Models.Requests;

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
}