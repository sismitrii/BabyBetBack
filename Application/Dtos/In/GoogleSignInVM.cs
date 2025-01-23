using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.In;

public class GoogleSignInVM
{
    [Required]
    public string IdToken { get; set; }
}