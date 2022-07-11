namespace InscricaoChll.Api.Models.Responses;

public class SignInResponse
{
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public UserModel User { get; set; }
}