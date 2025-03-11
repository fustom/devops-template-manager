using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;

namespace CloneDevOpsTemplate.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProjectService _projectService;

    public HomeController(ILogger<HomeController> logger, IProjectService projectService)
    {
        _logger = logger;
        _projectService = projectService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [Route("Home/Projects")]
    async public Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    [HttpGet]
    [Route("Home/ProjectProperties/{projectId}")]
    async public Task<IActionResult> ProjectProperties(string projectId)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    [HttpPost]
    [Route("Home/Login")]
    public IActionResult Login(LoginModel loginModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // update session with user credentials
                HttpContext.Session.SetString(Const.SessionKeyOrganizationName, loginModel.OrganizationName);
                HttpContext.Session.SetString(Const.SessionKeyAccessToken, loginModel.AccessToken);

                return Redirect("Projects");
                
                //string processTemplateType = projectProperties.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault()?.Value.ToString() ?? string.Empty;
                //await _projectService.CreateProjectAsync(processTemplateType);

                // requestUri = $"https://dev.azure.com/{orgName}/_apis/work/processes/{processTemplateType}";
                // Processes processes = await client.GetFromJsonAsync<Processes>(requestUri) ?? new Processes();
            }
            catch (Exception)
            {
                return Redirect("Error");
            }
        }

        return View("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
