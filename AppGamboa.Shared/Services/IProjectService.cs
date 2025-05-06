using AppGamboa.Shared.Models;

namespace AppGamboa.Shared.Services
{
    public interface IProjectService
    {
        Task<List<ProjectModel>> GetProjects();
    }
}
