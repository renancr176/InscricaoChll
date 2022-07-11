using System.ComponentModel.DataAnnotations;

namespace InscricaoChll.Api.Models.Requests;

public class SignUpRequest
{
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Name { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}