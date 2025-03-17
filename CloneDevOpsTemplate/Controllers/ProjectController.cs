using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class ProjectController(IProjectService projectService, IIterationService iterationService) : Controller
{
    private readonly IProjectService _projectService = projectService;
    private readonly IIterationService _iterationService = iterationService;

    async public Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    async public Task<IActionResult> Project(Guid projectId)
    {
        Project project = await _projectService.GetProjectAsync(projectId) ?? new Project();
        return View(project);
    }

    async public Task<IActionResult> ProjectProperties(Guid projectId)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    [HttpGet]
    async public Task<IActionResult> CreateProject()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    [HttpPost]
    async public Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName, string description, string visibility)
    {
        Project templateProject = await _projectService.GetProjectAsync(templateProjectId) ?? new();
        CreateProjectResponse createProjectResponse = await _projectService.CreateProjectAsync(newProjectName, description, templateProject.Capabilities.ProcessTemplate.TemplateTypeId, templateProject.Capabilities.Versioncontrol.SourceControlType, visibility) ?? new();

        if (createProjectResponse.Message is not null)
        {
            ModelState.AddModelError("ErrorMessage", createProjectResponse.Message);
            Projects projects = await _projectService.GetAllProjectsAsync() ?? new();
            return View(projects.Value);
        }

        Project project = new();
        while (project.State != "wellFormed")
        {
            await Task.Delay(1000);
            project = await _projectService.GetProjectAsync(newProjectName) ?? new();
        }

        Iteration templateIterations = await _iterationService.GetIterationsAsync(templateProjectId) ?? new();
        Iteration iterations = await _iterationService.CreateIterationAsync(project.Id, templateIterations);
        await _iterationService.MoveIteration(project.Id, iterations.Children);

         return RedirectToAction("Project", new { projectId = project.Id });
    }
}