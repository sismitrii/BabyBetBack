using System.ComponentModel.DataAnnotations;

namespace BabyBetBack.Auth;

public class JwtResponseVM
{
    [Required]
    public required string Token { get; set; }
}