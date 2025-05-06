using AppGamboa.Shared.Services;

namespace AppGamboa.Web.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "Web";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }

        public bool IsMobile() 
        {
            return false;
        }
    }
}
