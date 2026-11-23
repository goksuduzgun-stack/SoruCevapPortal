using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortal.Models;

public class Question
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
