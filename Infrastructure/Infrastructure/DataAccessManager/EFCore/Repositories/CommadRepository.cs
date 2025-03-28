﻿using Application.Common.Extensions;
using Application.Common.Repositories;
using Domain.Common;
using Infrastructure.DataAccessManager.EFCore.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccessManager.EFCore.Repositories;


public class CommandRepository<T> : ICommandRepository<T> where T : BaseEntity
{
    protected readonly CommandContext _context;

    public CommandRepository(CommandContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAtUtc = DateTime.UtcNow;
        await _context.AddAsync(entity, cancellationToken);
    }

    public async Task CreateListAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || entities.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.CreatedAtUtc = now;
        }

        // _context.ChangeTracker.AutoDetectChangesEnabled = false;

        await _context.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // _context.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    public void Create(T entity)
    {
        entity.CreatedAtUtc = DateTime.UtcNow;
        _context.Add(entity);
    }

    public void Update(T entity)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _context.Update(entity);
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _context.Update(entity);
    }

    public async Task DeleteListAsync(List<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null || entities.Count == 0) return;

        // On marque les entités à supprimer
        _context.RemoveRange(entities);

        // On applique la suppression dans la base de données
        await _context.SaveChangesAsync(cancellationToken);
    }


    public void Purge(T entity)
    {
        _context.Remove(entity);
    }

    public virtual async Task<T?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<T>()
            .IsDeletedEqualTo()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity;
    }

    public virtual T? Get(string id)
    {
        var entity = _context.Set<T>()
            .IsDeletedEqualTo()
            .SingleOrDefault(x => x.Id == id);

        return entity;
    }

    public virtual IQueryable<T> GetQuery()
    {
        var query = _context.Set<T>().AsQueryable();

        return query;
    }

    public async Task ClearDatabase(CancellationToken cancellationToken = default)
    {

        var allTables = _context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => t != null && !t.StartsWith("AspNet") && !t.StartsWith("Token") && !t.StartsWith("SalesTeam"))
            .ToList();

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            // Désactiver temporairement les contraintes de clés étrangères
            _context.Database.ExecuteSqlRaw("EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");

            foreach (var table in allTables)
            {
                var sql = $"DELETE FROM [{table}];";
                _context.Database.ExecuteSqlRaw(sql);
            }

            // Réactiver les contraintes
            _context.Database.ExecuteSqlRaw("EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");

            await _context.SaveChangesAsync(cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
