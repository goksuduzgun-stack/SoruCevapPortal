using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoruCevapPortal.Models;
using SoruCevapPortal.Repositories;

namespace SoruCevapPortal.Controllers;

[Authorize]
public class QuestionsController : Controller
{
    private readonly QuestionRepository _questionRepository;
    private readonly IRepository<Category> _categoryRepository;

    public QuestionsController(
        QuestionRepository questionRepository,
        IRepository<Category> categoryRepository)
    {
        _questionRepository = questionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = await GetCategoriesAsync(categoryId);
        return View(await _questionRepository.SearchAsync(search, categoryId));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? term, int? categoryId)
    {
        var questions = await _questionRepository.SearchAsync(term, categoryId);
        return Json(questions.Select(question => new
        {
            question.Id,
            question.Title,
            CategoryName = question.Category?.Name ?? "—",
            AnswerCount = question.Answers.Count,
            question.CreatedAt
        }));
    }

    public async Task<IActionResult> Add()
    {
        await SetCategoriesAsync();
        return View(new Question());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([Bind("Title,Content,CategoryId")] Question model)
    {
        if (!await CategoryExistsAsync(model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "Bir kategori seçin.");

        if (!ModelState.IsValid)
        {
            await SetCategoriesAsync(model.CategoryId);
            return View(model);
        }

        model.Title = model.Title.Trim();
        model.Content = model.Content.Trim();
        model.CreatedAt = DateTime.UtcNow;
        await _questionRepository.AddAsync(model);
        TempData["StatusMessage"] = "Soru eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
            return NotFound();

        await SetCategoriesAsync(question.CategoryId);
        return View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, [Bind("Title,Content,CategoryId")] Question model)
    {
        if (!await CategoryExistsAsync(model.CategoryId))
            ModelState.AddModelError(nameof(model.CategoryId), "Bir kategori seçin.");

        if (!ModelState.IsValid)
        {
            model.Id = id;
            await SetCategoriesAsync(model.CategoryId);
            return View(model);
        }

        model.Id = id;
        if (!await _questionRepository.UpdateAsync(model))
            return NotFound();

        TempData["StatusMessage"] = "Soru güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
            return NotFound();

        return View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _questionRepository.DeleteAsync(id))
            return NotFound();

        TempData["StatusMessage"] = "Soru ve ilişkili cevapları silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoryExistsAsync(int id) =>
        await _categoryRepository.GetByIdAsync(id) != null;

    private async Task SetCategoriesAsync(int? selectedId = null)
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories.OrderBy(category => category.Name), "Id", "Name", selectedId);
    }

    private async Task<SelectList> GetCategoriesAsync(int? selectedId = null)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return new SelectList(categories.OrderBy(category => category.Name), "Id", "Name", selectedId);
    }
}
