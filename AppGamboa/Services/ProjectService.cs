using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Services
{
    public class ProjectService : IProjectService
    {
        public async Task<List<ProjectModel>> GetProjectsAsync()
        {
            await Task.Delay(200); // Simula operação assíncrona real

            return new List<ProjectModel>
            {
                new ProjectModel { Title = "AppGamboa", Description = "Aplicativo híbrido para gestão." },
                new ProjectModel { Title = "CRM Gamboa", Description = "Automação de vendas e atendimento." },
                new ProjectModel { Title = "Data Platform", Description = "Integração de dados multicanal." }
            };
        }
    }
}
