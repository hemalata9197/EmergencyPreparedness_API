using System.IO;
using Microsoft.AspNetCore.Hosting;
namespace EmergencyManagement.Services.Implementations
{
    public class EmailTemplateService
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public string LoadTemplate(string fileName)
        {
            var path = Path.Combine(_env.ContentRootPath, "Templates", "Emails", fileName);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public string BuildHeader( string username)
        {
            string template = LoadTemplate("HTMLHeader.html");
            return template
                
                .Replace("{{username}}", username)
                .Replace("{{date}}", DateTime.Now.ToString("dd.MMM.yyyy"));
        }

        public string BuildFooter()
        {
            return LoadTemplate("HTMLFooter.html");
        }
    }
}
