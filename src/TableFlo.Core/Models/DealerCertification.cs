using TableFlo.Core.Enums;

namespace TableFlo.Core.Models;

/// <summary>
/// Represents a dealer's certification for a specific game type
/// </summary>
public class DealerCertification
{
    public int Id { get; set; }
    
    public int DealerId { get; set; }
    
    public Dealer? Dealer { get; set; }
    
    public GameType GameType { get; set; }
    
    public ProficiencyLevel ProficiencyLevel { get; set; }
    
    /// <summary>
    /// For Craps dealers - specific role certification
    /// </summary>
    public CrapsRole CrapsRole { get; set; } = CrapsRole.None;
    
    public DateTime CertifiedDate { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    public bool IsActive { get; set; } = true;
}

