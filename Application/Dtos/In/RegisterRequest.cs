namespace Application.Dtos.In;

public class RegisterRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    // to add profile_picture

    public override string ToString()
        => $"FirstName: {FirstName}\nLastName: {LastName}\nEmail: {Email}\nPassword: {Password}";
}