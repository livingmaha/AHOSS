// /Assets/Scripts/SimulationManager.cs

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text; // Required for Encoding
using System.Linq; // Required for FindObjectsOfType

/// <summary>
/// Serializable classes to match the JSON structure for communication with the Python AI.
/// </summary>
[System.Serializable]
public class EntityData
{
    public string id;
    public string type;
    public Vector3 position;
}

[System.Serializable]
public class SimulationPayload
{
    public List<EntityData> entities = new List<EntityData>();
}

[System.Serializable]
public class AIResponse
{
    public string target_id;
}


/// <summary>
/// Orchestrates the simulation, collects data, and communicates with the external Python AI.
/// </summary>
public class SimulationManager : MonoBehaviour
{
    [Header("AI Connection")]
    [Tooltip("The URL of the Python Flask server.")]
    private string aiServerUrl = "http://127.0.0.1:5000/predict";
    
    [Tooltip("How often (in seconds) to send data to the AI.")]
    public float updateInterval = 0.5f;

    [Header("References")]
    [Tooltip("Reference to the UIManager to update the HUD.")]
    public UIManager uiManager;

    // A dictionary to quickly find an Entity object by its ID string.
    private Dictionary<string, Entity> entityRegistry = new Dictionary<string, Entity>();

    void Start()
    {
        // Start the main simulation loop
        StartCoroutine(SimulationLoop());
    }

    /// <summary>
    /// The main loop that periodically scans the environment and queries the AI.
    /// </summary>
    private IEnumerator SimulationLoop()
    {
        while (true)
        {
            // 1. Scan the scene for all entities
            var allEntities = FindObjectsOfType<Entity>();
            
            // Update our registry for quick lookups
            entityRegistry.Clear();
            foreach (var entity in allEntities)
            {
                if (!entityRegistry.ContainsKey(entity.entityId))
                {
                    entityRegistry.Add(entity.entityId, entity);
                }
            }
            
            // 2. Package data into a JSON string
            string jsonData = PackageEntityData(allEntities);

            // 3. Send data to the Python AI and wait for a response
            yield return SendDataToAI(jsonData);
            
            // 4. Wait for the next interval
            yield return new WaitForSeconds(updateInterval);
        }
    }

    /// <summary>
    /// Collects data from all entities in the scene and serializes it to JSON.
    /// </summary>
    private string PackageEntityData(Entity[] entities)
    {
        SimulationPayload payload = new SimulationPayload();

        foreach (var entity in entities)
        {
            EntityData data = new EntityData
            {
                id = entity.entityId,
                type = entity.entityType.ToString(),
                position = entity.transform.position
            };
            payload.entities.Add(data);
        }
        
        return JsonUtility.ToJson(payload);
    }
    
    /// <summary>
    /// Sends the simulation data to the Python server and handles the response.
    /// </summary>
    private IEnumerator SendDataToAI(string json)
    {
        // Using UnityWebRequest for modern web requests
        using (UnityWebRequest request = new UnityWebRequest(aiServerUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Request was successful, process the response
                string responseJson = request.downloadHandler.text;
                AIResponse aiResponse = JsonUtility.FromJson<AIResponse>(responseJson);
                
                Debug.Log("AI responded with target ID: " + aiResponse.target_id);
                
                // Find the entity corresponding to the target ID and update the UI
                ProcessAIResponse(aiResponse.target_id);
            }
            else
            {
                // An error occurred
                Debug.LogError("Error contacting AI server: " + request.error);
                // Clear the target if we can't reach the AI
                uiManager.SetDesignatedTarget(null);
            }
        }
    }

    /// <summary>
    /// Finds the target entity by its ID and tells the UIManager to highlight it.
    /// </summary>
    private void ProcessAIResponse(string targetId)
    {
        if (!string.IsNullOrEmpty(targetId) && entityRegistry.ContainsKey(targetId))
        {
            Entity targetEntity = entityRegistry[targetId];
            uiManager.SetDesignatedTarget(targetEntity);
        }
        else
        {
            // If the AI returns a null or invalid ID, clear the current target.
            uiManager.SetDesignatedTarget(null);
        }
    }
}
