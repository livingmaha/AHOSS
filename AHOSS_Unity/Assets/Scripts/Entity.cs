// /Assets/Scripts/Entity.cs

using UnityEngine;
using System; // Required for Guid

/// <summary>
/// Defines the type of an entity in the simulation.
/// </summary>
public enum EntityType
{
    AHOSS_Craft,
    Enemy_Unit,
    Friendly_Unit,
    Civilian_Unit
}

/// <summary>
/// Attached to all dynamic objects in the simulation (enemies, friendlies, civilians, AHOSS).
/// Holds core identification data.
/// </summary>
public class Entity : MonoBehaviour
{
    [Tooltip("The type of this entity.")]
    public EntityType entityType;

    [Tooltip("Unique identifier for this entity instance.")]
    [ReadOnly] // Makes it visible but not editable in the Inspector
    public string entityId;

    /// <summary>
    /// When the object is first created, assign a new unique ID.
    /// </summary>
    void Awake()
    {
        // Generate a globally unique identifier to ensure the AI can distinguish
        // between two entities of the same type.
        entityId = Guid.NewGuid().ToString();
    }
}
