using System.Collections.Generic;
using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Shared.ViewModels
{
    public class HomeViewModel
    {
        private readonly IFormFactor _formFactor;

        public HomeViewModel(IFormFactor formFactor)
        {
            _formFactor = formFactor;
        }

        public bool IsMobile => _formFactor.IsMobile();

        public string HeroTitle => "Soluções Digitais de Alta Qualidade";

        public string HeroSubtitle => "Transformamos ideias em software de excelência usando as melhores práticas de arquitetura e desenvolvimento.";
    }
}