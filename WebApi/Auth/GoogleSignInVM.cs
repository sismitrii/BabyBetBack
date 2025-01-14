using System.ComponentModel.DataAnnotations;

namespace BabyBetBack.Auth;

public class GoogleSignInVM
{
    [Required]
    public string IdToken { get; set; }
}