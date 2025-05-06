using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

public class ProjectsViewModel
{
    private readonly IProjectService _projectService;
    private List<ProjectModel> _projects;

    public ProjectsViewModel(IProjectService projectService)
    {
        _projectService = projectService;
        _projects = new List<ProjectModel>(); // Inicializa como lista vazia em vez de null
        LoadProjects();
    }

    public List<ProjectModel> Projects => _projects ?? new List<ProjectModel>();

    private async void LoadProjects()
    {
        try
        {
            var projects = await _projectService.GetProjects();
            _projects = projects ?? new List<ProjectModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar projetos: {ex.Message}");
            _projects = new List<ProjectModel>();
        }
    }
}