using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class InteractableStructure : MonoBehaviour
{
    public List<GameObject> lootingWaypoints;
    public List<GameObject> defensiveCoverWaypoints;
    public List<GameObject> attackCoverWaypoints;

    public List<GameObject> aiInBuilding;

    public int radius;
    public Vector3 position;
    private AITriggerManager triggers;

    // Use this for initialization
    void Start() {
        triggers = gameObject.AddComponent<AITriggerManager>();
        triggers.AddTrigger("Interactable Structure Trigger Node", radius, TriggerEnter, TriggerExit);

        aiInBuilding = new List<GameObject>();
    }

    public bool CheckForAIInBuilding(GameObject ai) {
        return aiInBuilding.Contains(ai);
    }

    public void TriggerEnter(Collider other) {
        GameObject player = other.gameObject;
        AIMovementManager ai = player.GetComponent<AIMovementManager>();
        if (ai != null && !aiInBuilding.Contains(ai.gameObject))
        {
            ai.StructureReached(gameObject);
            aiInBuilding.Add(player);
        }
    }

    public void TriggerExit(Collider other) {
        GameObject player = other.gameObject;
        AIMovementManager ai = player.GetComponent<AIMovementManager>();
        if (ai != null && aiInBuilding.Contains(ai.gameObject))
            aiInBuilding.Remove(player);
    }
}

