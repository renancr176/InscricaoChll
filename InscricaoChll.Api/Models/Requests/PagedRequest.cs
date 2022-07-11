using System.ComponentModel.DataAnnotations;

namespace InscricaoChll.Api.Models.Requests;

public abstract class PagedRequest
{
    [Range(1, int.MaxValue)]
    public int PageIndex { get; set; } = 1;
    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 50;
}