namespace InscricaoChll.Api.Interfaces.Services;

public interface ITemplateService
{
    Task<string> GetContent(string templateFile, Dictionary<string, string> replaces = null);
}