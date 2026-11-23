using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoruCevapPortal.Models;
using SoruCevapPortal.Repositories;
using SoruCevapPortal.ViewModels;

namespace SoruCevapPortal.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IRepository<Question> _questions;
    private readonly IRepository<Answer> _answers;
    private readonly IRepository<Category> _categories;

    public AdminController(
        IRepository<Question> questions,
        IRepository<Answer> answers,
        IRepository<Category> categories)
    {
        _questions = questions;
        _answers = answers;
        _categories = categories;
    }

    public async Task<IActionResult> Index()
    {
        var questions = await _questions.GetAllAsync();
        var answers = await _answers.GetAllAsync();
        var categories = await _categories.GetAllAsync();

        return View(new AdminDashboardViewModel
        {
            QuestionCount = questions.Count,
            AnswerCount = answers.Count,
            CategoryCount = categories.Count
        });
    }

    public async Task<IActionResult> Questions(string? search, int? categoryId)
    {
        return View(await BuildQuestionIndex(search, categoryId));
    }

    [HttpGet]
    public async Task<IActionResult> SearchQuestions(string? term, int? categoryId)
    {
        var model = await BuildQuestionIndex(term, categoryId);
        return Json(model.Questions);
    }

    public async Task<IActionResult> CreateQuestion()
    {
        return View(await BuildQuestionForm(new AdminQuestionFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(AdminQuestionFormViewModel model)
    {
        if (!await CategoryExists(model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "Bir kategori seçin.");

        if (!ModelState.IsValid)
            return View(await BuildQuestionForm(model));

        await _questions.AddAsync(new Question
        {
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            CategoryId = model.CategoryId
        });
        await _questions.SaveAsync();
        TempData["StatusMessage"] = "Soru eklendi.";
        return RedirectToAction(nameof(Questions));
    }

    public async Task<IActionResult> EditQuestion(int id)
    {
        var question = await _questions.GetByIdAsync(id);
        if (question == null) return NotFound();

        return View(await BuildQuestionForm(new AdminQuestionFormViewModel
        {
            Id = question.Id,
            Title = question.Title,
            Content = question.Content,
            CategoryId = question.CategoryId
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(AdminQuestionFormViewModel model)
    {
        var question = await _questions.GetByIdAsync(model.Id);
        if (question == null) return NotFound();

        if (!await CategoryExists(model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "Bir kategori seçin.");

        if (!ModelState.IsValid)
            return View(await BuildQuestionForm(model));

        question.Title = model.Title.Trim();
        question.Content = model.Content.Trim();
        question.CategoryId = model.CategoryId;
        _questions.Update(question);
        await _questions.SaveAsync();
        TempData["StatusMessage"] = "Soru güncellendi.";
        return RedirectToAction(nameof(Questions));
    }

    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _questions.GetByIdAsync(id);
        if (question == null) return NotFound();

        var categories = await _categories.GetAllAsync();
        return View(new AdminQuestionItemViewModel
        {
            Id = question.Id,
            Title = question.Title,
            CategoryName = categories.FirstOrDefault(c => c.Id == question.CategoryId)?.Name ?? "—",
            CreatedAt = question.CreatedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDeleteQuestion(int id)
    {
        var question = await _questions.GetByIdAsync(id);
        if (question == null) return NotFound();

        _questions.Remove(question);
        await _questions.SaveAsync();
        TempData["StatusMessage"] = "Soru silindi.";
        return RedirectToAction(nameof(Questions));
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

        var categories = await _categories.GetAllAsync();
        if (categories.Any(c => string.Equals(c.Name, model.NewCategoryName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.NewCategoryName), "Bu kategori zaten var.");
            return View(nameof(Categories), await BuildCategoryManagement(model.NewCategoryName));
        }

        await _categories.AddAsync(new Category { Name = model.NewCategoryName.Trim() });
        await _categories.SaveAsync();
        TempData["StatusMessage"] = "Kategori eklendi.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categories.GetByIdAsync(id);
        if (category == null) return NotFound();

        var questions = await _questions.FindAsync(q => q.CategoryId == id);
        if (questions.Count != 0)
        {
            TempData["ErrorMessage"] = "Bu kategoride soru olduğu için kategori silinemedi.";
            return RedirectToAction(nameof(Categories));
        }

        _categories.Remove(category);
        await _categories.SaveAsync();
        TempData["StatusMessage"] = "Kategori silindi.";
        return RedirectToAction(nameof(Categories));
    }

    private async Task<AdminQuestionIndexViewModel> BuildQuestionIndex(string? search, int? categoryId)
    {
        var questions = await _questions.GetAllAsync();
        var categories = await _categories.GetAllAsync();
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var filtered = questions.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filtered = filtered.Where(q =>
                q.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                q.Content.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        if (categoryId.HasValue)
            filtered = filtered.Where(q => q.CategoryId == categoryId.Value);

        return new AdminQuestionIndexViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Categories = categories
                .OrderBy(c => c.Name)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = categoryId == c.Id
                }).ToList(),
            Questions = filtered
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new AdminQuestionItemViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    CategoryName = categoryNames.TryGetValue(q.CategoryId, out var name) ? name : "—",
                    CreatedAt = q.CreatedAt
                }).ToList()
        };
    }

    private async Task<AdminQuestionFormViewModel> BuildQuestionForm(AdminQuestionFormViewModel model)
    {
        model.Categories = (await _categories.GetAllAsync())
            .OrderBy(c => c.Name)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            }).ToList();
        return model;
    }

    private async Task<AdminCategoryManagementViewModel> BuildCategoryManagement(string newCategoryName)
    {
        var categories = await _categories.GetAllAsync();
        var questions = await _questions.GetAllAsync();
        return new AdminCategoryManagementViewModel
        {
            NewCategoryName = newCategoryName,
            Categories = categories
                .OrderBy(c => c.Name)
                .Select(c => new AdminCategoryItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    QuestionCount = questions.Count(q => q.CategoryId == c.Id)
                }).ToList()
        };
    }

    private async Task<bool> CategoryExists(int id) =>
        (await _categories.GetByIdAsync(id)) != null;
}
