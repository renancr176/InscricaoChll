using System.Linq.Expressions;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;
using InscricaoChll.Api.DbContexts.ChllDbContext.Interfaces.Repositories;
using InscricaoChll.Api.Models.Requests;

namespace InscricaoChll.Api.Models.Responses;

public class PagedResponse<TData> : BaseResponse<IEnumerable<TData>>
{
    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public PagedResponse()
    {
    }

    public PagedResponse(PagedRequest request)
    {
        PageIndex = request.PageIndex;
        PageSize = request.PageSize;
    }

    public async Task SetTotals<TEntity>(IRepository<TEntity> repository,
        Expression<Func<TEntity, bool>> predicate = null)
        where TEntity : Entity<TEntity>
    {
        TotalCount = await repository.CountAsync(predicate ?? (entity => true));
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}