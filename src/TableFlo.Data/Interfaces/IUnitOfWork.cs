using TableFlo.Core.Models;

namespace TableFlo.Data.Interfaces;

/// <summary>
/// Unit of Work pattern for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Employee> Employees { get; }
    IRepository<Dealer> Dealers { get; }
    IRepository<DealerCertification> DealerCertifications { get; }
    IRepository<Table> Tables { get; }
    IRepository<Assignment> Assignments { get; }
    IRepository<BreakRecord> BreakRecords { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Shift> Shifts { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

