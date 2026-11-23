using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SoruCevapPortal.ViewModels;

public class AdminQuestionIndexViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
    public List<AdminQuestionItemViewModel> Questions { get; set; } = new();
}

public class AdminQuestionItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminQuestionFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    [Display(Name = "İçerik")]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Bir kategori seçin.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();
}
