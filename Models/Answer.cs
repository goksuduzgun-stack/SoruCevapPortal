using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortal.Models;

public class Answer
{
    public int Id { get; set; }

    [Required, StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}
