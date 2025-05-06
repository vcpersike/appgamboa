using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Web.Services
{
    public class ProjectService : IProjectService
    {
        public Task<List<ProjectModel>> GetProjects()
        {
            var projects = new List<ProjectModel>
            {
                new ProjectModel { Title = "AppGamboa", Description = "Aplicativo híbrido para gestão." },
                new ProjectModel { Title = "CRM Gamboa", Description = "Automação de vendas e atendimento." },
                new ProjectModel { Title = "Data Platform", Description = "Integração de dados multicanal." }
            };

            return Task.FromResult(projects);
        }
    }
}
