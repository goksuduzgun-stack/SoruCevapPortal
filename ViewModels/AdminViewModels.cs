using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortal.ViewModels;

public class AdminDashboardViewModel
{
    public int QuestionCount { get; set; }
    public int AnswerCount { get; set; }
    public int CategoryCount { get; set; }
}

public class AdminCategoryItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

public class AdminCategoryManagementViewModel
{
    [Required, StringLength(80)]
    [Display(Name = "Kategori adı")]
    public string NewCategoryName { get; set; } = string.Empty;

    public List<AdminCategoryItemViewModel> Categories { get; set; } = new();
}
