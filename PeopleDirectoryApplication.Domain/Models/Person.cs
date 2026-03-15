using System.ComponentModel.DataAnnotations;
using PeopleDirectoryApplication.Data.Enum;

namespace PeopleDirectoryApplication.Models;

public class Person
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Url]
    [MaxLength(500)]
    public string? ProfilePicture { get; set; }

    [Phone]
    [MaxLength(30)]
    public string? MobileNumber { get; set; }

    public Gender Gender { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? EmailAddress { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
