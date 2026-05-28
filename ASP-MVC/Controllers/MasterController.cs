using ASP_MVC.Services;
using ASP_MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MVC.Controllers;

/// <summary>
/// Company / User / Favorite の通常CRUD画面を提供するController。
/// </summary>
public class MasterController : Controller
{
    private readonly MasterCrudService _masterCrudService;

    public MasterController(MasterCrudService masterCrudService)
    {
        _masterCrudService = masterCrudService;
    }

    [HttpGet]
    public IActionResult Companies(int? editId)
    {
        return View(_masterCrudService.BuildCompanyPage(editId, message: TempData["Message"] as string));
    }

    [HttpPost]
    public IActionResult SaveCompany(CrudCompanyInputViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View("Companies", _masterCrudService.BuildCompanyPage(draft: input, message: "入力内容を確認してください。"));
        }

        TempData["Message"] = _masterCrudService.SaveCompany(input);
        return RedirectToAction(nameof(Companies));
    }

    [HttpPost]
    public IActionResult DeleteCompany(int id)
    {
        TempData["Message"] = _masterCrudService.DeleteCompany(id);
        return RedirectToAction(nameof(Companies));
    }

    [HttpGet]
    public IActionResult Users(int? editId)
    {
        return View(_masterCrudService.BuildUserPage(editId, message: TempData["Message"] as string));
    }

    [HttpPost]
    public IActionResult SaveUser(CrudUserInputViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View("Users", _masterCrudService.BuildUserPage(draft: input, message: "入力内容を確認してください。"));
        }

        TempData["Message"] = _masterCrudService.SaveUser(input);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        TempData["Message"] = _masterCrudService.DeleteUser(id);
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult Favorites(int? editId)
    {
        return View(_masterCrudService.BuildFavoritePage(editId, message: TempData["Message"] as string));
    }

    [HttpPost]
    public IActionResult SaveFavorite(CrudFavoriteInputViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View("Favorites", _masterCrudService.BuildFavoritePage(draft: input, message: "入力内容を確認してください。"));
        }

        TempData["Message"] = _masterCrudService.SaveFavorite(input);
        return RedirectToAction(nameof(Favorites));
    }

    [HttpPost]
    public IActionResult DeleteFavorite(int id)
    {
        TempData["Message"] = _masterCrudService.DeleteFavorite(id);
        return RedirectToAction(nameof(Favorites));
    }
}
