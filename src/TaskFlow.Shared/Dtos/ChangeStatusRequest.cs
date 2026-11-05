using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Shared.Dtos;

public sealed class ChangeStatusRequest
{
    // Nullable, altfel [Required] nu observă câmpul lipsă (ar veni Todo)
    [Required]
    [EnumDataType(typeof(TaskItemStatus))]
    public TaskItemStatus? Status { get; set; }
}