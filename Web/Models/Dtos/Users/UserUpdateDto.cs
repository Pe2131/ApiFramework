
using Entities;
using System.ComponentModel.DataAnnotations;
using WebFramework.Api;
using static Common.Enums;

namespace Models.Dtos.Users
{
    public class UserUpdateDto : BaseDto<UserUpdateDto, User, string>, IValidatableObject
    {
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        public string image { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public string City { get; set; }

        public GenderType Gender { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password.Equals("123456"))
                yield return new ValidationResult("your password cant be 12345", new[] { nameof(Password) });
            //if (Gender == GenderType.Male && Age > 30)
            //    yield return new ValidationResult("آقایان بیشتر از 30 سال معتبر نیستند", new[] { nameof(Gender), nameof(Age) });
        }
    }
}
