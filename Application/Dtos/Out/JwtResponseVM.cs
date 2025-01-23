using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Out;
public class JwtResponseDto
{
    [Required]
    public required string Token { get; set; }
}