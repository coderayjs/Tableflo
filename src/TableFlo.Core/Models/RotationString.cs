namespace TableFlo.Core.Models;

/// <summary>
/// Represents a rotation string/group - predefined dealer rotation groups
/// Similar to "Craps", "HL", "String4", "String5" in legacy systems
/// </summary>
public class RotationString
{
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the rotation string (e.g., "Craps", "HighLimit", "MainFloor", "String1")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this string covers
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Is this string currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Priority order for scheduling (lower = higher priority)
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// Dealers assigned to this rotation string
    /// </summary>
    public List<DealerStringAssignment> DealerAssignments { get; set; } = new();
    
    /// <summary>
    /// Tables covered by this rotation string
    /// </summary>
    public List<int> TableIds { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Links dealers to rotation strings
/// </summary>
public class DealerStringAssignment
{
    public int Id { get; set; }
    
    public int DealerId { get; set; }
    public Dealer? Dealer { get; set; }
    
    public int RotationStringId { get; set; }
    public RotationString? RotationString { get; set; }
    
    /// <summary>
    /// Order within the rotation string (for rotation sequence)
    /// </summary>
    public int RotationOrder { get; set; }
    
    /// <summary>
    /// Is this assignment currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}

