using System.ComponentModel.DataAnnotations;
using PeopleDirectoryApplication.Data.Enum;

namespace PeopleDirectoryApplication.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? ProfilePicture { get; set; }
        public string? MobileNumber { get; set; }
        public Gender Gender { get; set; }
        public string? EmailAddress { get; set; }
    }
}
