using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Shared.Dtos;

public sealed record CreateTagRequest(
    [Required]
    [StringLength(50)]
    string Name);