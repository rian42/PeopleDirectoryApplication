using System.ComponentModel.DataAnnotations;

namespace PeopleDirectoryApplication.Models;

public class AuditTrailEntry
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAtUtc { get; set; }

    [Required]
    public string ChangesJson { get; set; } = string.Empty;
}
