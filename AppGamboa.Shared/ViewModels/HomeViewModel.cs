using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboa.Shared.ViewModels
{
    public class HomeViewModel
    {
        public ContactViewModel Contact { get; }
        public ProjectsViewModel Projects { get; }

        public HomeViewModel(ContactViewModel contact, ProjectsViewModel projects)
        {
            Contact = contact;
            Projects = projects;
        }
    }

}

