using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortal.Models;
using SoruCevapPortal.Repositories;
using SoruCevapPortal.ViewModels;

namespace SoruCevapPortal.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly QuestionRepository _questionRepository;
    private readonly IRepository<Answer> _answerRepository;
    private readonly IRepository<Category> _categoryRepository;

    public AdminController(
        QuestionRepository questionRepository,
        IRepository<Answer> answerRepository,
        IRepository<Category> categoryRepository)
    {
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index()
    {
        var questions = await _questionRepository.GetListAsync();
        var answers = await _answerRepository.GetAllAsync();
        var categories = await _categoryRepository.GetAllAsync();

        return View(new AdminDashboardViewModel
        {
            QuestionCount = questions.Count,
            AnswerCount = answers.Count,
            CategoryCount = categories.Count
        });
    }

    public async Task<IActionResult> Categories()
    {
        return View(await BuildCategoryManagement(string.Empty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(AdminCategoryManagementViewModel model)
    {
        if (!ModelState.IsValid)
            return View(nameof(Categories), await BuildCategoryManagement(model.NewCategoryName));

        var categories = await _categoryRepository.GetAllAsync();
        if (categories.Any(category =>
            string.Equals(category.Name, model.NewCategoryName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.NewCategoryName), "Bu kategori zaten var.");
            return View(nameof(Categories), await BuildCategoryManagement(model.NewCategoryName));
        }

        await _categoryRepository.AddAsync(new Category { Name = model.NewCategoryName.Trim() });
        await _categoryRepository.SaveAsync();
        TempData["StatusMessage"] = "Kategori eklendi.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return NotFound();

        var questions = await _questionRepository.GetListAsync();
        if (questions.Any(question => question.CategoryId == id))
        {
            TempData["ErrorMessage"] = "Bu kategoride soru olduğu için kategori silinemedi.";
            return RedirectToAction(nameof(Categories));
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveAsync();
        TempData["StatusMessage"] = "Kategori silindi.";
        return RedirectToAction(nameof(Categories));
    }

    private async Task<AdminCategoryManagementViewModel> BuildCategoryManagement(string newCategoryName)
    {
        var categories = await _categoryRepository.GetAllAsync();
        var questions = await _questionRepository.GetListAsync();
        return new AdminCategoryManagementViewModel
        {
            NewCategoryName = newCategoryName,
            Categories = categories
                .OrderBy(category => category.Name)
                .Select(category => new AdminCategoryItemViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    QuestionCount = questions.Count(question => question.CategoryId == category.Id)
                }).ToList()
        };
    }
}
