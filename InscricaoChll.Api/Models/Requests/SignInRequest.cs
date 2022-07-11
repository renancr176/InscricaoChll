using System.ComponentModel.DataAnnotations;

namespace InscricaoChll.Api.Models.Requests;

public class SignInRequest
{
    [Required(ErrorMessage = "O login deve ser informado")]
    [EmailAddress(ErrorMessage = "O login deve ser uma e-mail válido")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(8, ErrorMessage = "A senha de conter ao menos 8 caracteres.")]
    public string Password { get; set; }
}