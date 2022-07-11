using InscricaoChll.Api.DbContexts.ChllDbContext.Enums;
using Microsoft.AspNetCore.Identity;

namespace InscricaoChll.Api.DbContexts.ChllDbContext.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public string Name { get; set; }
    public UserStatusEnum Status { get; set; } = UserStatusEnum.Active;
    public string Token { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public string Test { get; set; }
}