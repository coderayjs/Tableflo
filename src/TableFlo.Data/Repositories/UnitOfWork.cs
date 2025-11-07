using Microsoft.EntityFrameworkCore.Storage;
using TableFlo.Core.Models;
using TableFlo.Data.Interfaces;

namespace TableFlo.Data.Repositories;

/// <summary>
/// Unit of Work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TableFloDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TableFloDbContext context)
    {
        _context = context;
        
        Employees = new Repository<Employee>(_context);
        Dealers = new Repository<Dealer>(_context);
        DealerCertifications = new Repository<DealerCertification>(_context);
        Tables = new Repository<Table>(_context);
        Assignments = new Repository<Assignment>(_context);
        BreakRecords = new Repository<BreakRecord>(_context);
        AuditLogs = new Repository<AuditLog>(_context);
        Shifts = new Repository<Shift>(_context);
    }

    public IRepository<Employee> Employees { get; }
    public IRepository<Dealer> Dealers { get; }
    public IRepository<DealerCertification> DealerCertifications { get; }
    public IRepository<Table> Tables { get; }
    public IRepository<Assignment> Assignments { get; }
    public IRepository<BreakRecord> BreakRecords { get; }
    public IRepository<AuditLog> AuditLogs { get; }
    public IRepository<Shift> Shifts { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

