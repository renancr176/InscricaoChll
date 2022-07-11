using InscricaoChll.Api.Interfaces;
using InscricaoChll.Api.Interfaces.Services;
using InscricaoChll.Api.Options;
using Microsoft.Extensions.Options;

namespace InscricaoChll.Api.Services;

public class TemplateService : ITemplateService
{
    private readonly IOptions<GeneralOptions> _options;

    public GeneralOptions Options => _options.Value;

    public TemplateService(IOptions<GeneralOptions> options)
    {
        _options = options;
    }

    public async Task<string> GetContent(string templateFile, Dictionary<string, string> replaces = null)
    {
        try
        {
            //Path.PathSeparator

            var folderPath = Options.TemplateFolderPath
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .Replace("\\", Path.DirectorySeparatorChar.ToString());

            if (folderPath.EndsWith("/")
                || folderPath.EndsWith("\\"))
            {
                folderPath = folderPath.Substring(0, folderPath.Length - 1);
            }

            var path = $@"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{folderPath}{Path.DirectorySeparatorChar}{templateFile}";

            if (File.Exists(path))
            {
                var templateContent = File.ReadAllText(path);

                if (!string.IsNullOrEmpty(templateContent) && replaces != null)
                {
                    foreach (var replace in replaces)
                    {
                        templateContent = templateContent.Replace(replace.Key, replace.Value);
                    }
                }

                return templateContent;
            }
        }
        catch (Exception e)
        {
        }

        return default;
    }
}