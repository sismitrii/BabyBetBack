using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

    [Table(nameof(User))]
    public class User : IdentityUser<long>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{LastName} {FirstName}";
            }
        }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"FirstName : '{FirstName}'," +
                   $" LastName : '{LastName}'," +
                   $" Email : '{Email}'," +
                   $" UserName : '{UserName}'," +
                   $" EmailConfirmed : '{EmailConfirmed}'," +
                   $" ProfilePicture : '{ProfilePicture}'";
        }
            
    }

    public class Role : IdentityRole<long>
    {
    }

    public class UserClaim : IdentityUserClaim<long> { }

    public class UserRole : IdentityUserRole<long> { }

    public class UserLogin : IdentityUserLogin<long> { }

    public class RoleClaim : IdentityRoleClaim<long> { }

    public class UserToken : IdentityUserToken<long> { }