using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Shared.ViewModels
{
    public class ProjectsViewModel
    {
        private readonly IProjectService _projectService;

        public List<ProjectModel> Projects { get; set; } = new();

        public ProjectsViewModel(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task LoadProjectsAsync()
        {
            Projects = await _projectService.GetProjectsAsync();
        }
    }
}
