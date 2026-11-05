using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Shared.Dtos;

public sealed record UpdateProjectRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Name);