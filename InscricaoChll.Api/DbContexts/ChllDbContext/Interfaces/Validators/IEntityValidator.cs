using FluentValidation;
using FluentValidation.Results;
using InscricaoChll.Api.DbContexts.ChllDbContext.Entities;

namespace InscricaoChll.Api.DbContexts.ChllDbContext.Interfaces.Validators;

public interface IEntityValidator<TEntity> : IValidator<TEntity> where TEntity : Entity<TEntity>
{
    ValidationResult ValidationResult { get; }
    bool IsValid(TEntity entity);
}