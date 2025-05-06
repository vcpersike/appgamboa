using System.Collections.Generic;

namespace AppGamboa.Shared.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public List<string> Technologies { get; set; }
    }
}