using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AICharacterController))]

public class AIMovementManager : MonoBehaviour {

    private AICharacterController controller;
    private EnemyAI ai;
    public List<GameObject> currentTargets;
    bool pause = false;

    // Use this for initialization
    private void Awake() {
        ai = gameObject.GetComponent<EnemyAI>();
    }

    void Start () {
        controller = GetComponent<AICharacterController>();
        gameObject.GetComponent<AITriggerManager>().AddTrigger("Movement Trigger", 0.3f, TargetReachedTriggerFunc, true);
        if (currentTargets == null)
            currentTargets = new List<GameObject>();
    }

    public void StructureReached(GameObject waypoint)
    {
        InteractableStructure building = waypoint.GetComponent<InteractableStructure>();
        if (building != null && RemoveTarget(building.gameObject)) // it is a building && its on its list of buildings to travel to
            ai.OnEnterBuilding(building);
    }

    public void TargetReachedTriggerFunc(Collider col) { TargetReached(col.gameObject); } // passed to trigger node to direct to next function
    public void TargetReached(GameObject target) {
        if (target.CompareTag("Waypoint")) // is it a waypoint?
            if (target.transform == controller.GetTarget()) // is it the right waypoint?
                    ai.TargetReached(target);
        else if (target.CompareTag("Temporary")) {
                Destroy(controller.tempObject); 
                TargetReached(controller.originalObject);
            }
    }

    public void RunInDirection(Vector3 direction) {
        controller.RunInDirection(direction);
    }


    // target manipulation

    IEnumerator CheckForTargetWithSort() {
        currentTargets.Sort(SortByDistance);
        CheckForTarget();
        yield return null;
    }

    public void CheckForTarget() {
        if (pause)
            return;

        if (controller == null)
            controller = GetComponent<AICharacterController>();

        if (currentTargets.Count > 0) {
            foreach (GameObject target in currentTargets)
                if (target != null)
                    controller.SetTarget(target.transform);
            ai.ResetTriggers();
        } else
            controller.SetTarget(gameObject.transform);
    }


    public void NextTarget() {
        RemoveTarget(0);
    }

    public void SetManualTarget(GameObject target) {
        if (controller.target != null && target == GetCurrentTarget())
            return;

        ClearTargets();
        AddTarget(target);
    }

    public void AddTarget(GameObject target, int index = 0) {
        if (index <= currentTargets.Count && target != null)
            currentTargets.Insert(index, target);

        StartCoroutine(CheckForTargetWithSort());
    }

    public void AddTargets(List<GameObject> newTargets, int index = 0) {
        int counter = index;
        foreach (GameObject target in newTargets) {
            if (index <= currentTargets.Count && target != null)
                currentTargets.Insert(index, target);
            counter++;
        }

        StartCoroutine(CheckForTargetWithSort());
    }

    public bool RemoveTarget(GameObject target) {
        if (currentTargets.Count > 0) {
            GameObject current = currentTargets[0];

            // if target was a temp object because original wasn't reachable
            if (current != null && current.transform.parent != null)
                if (target.transform == current.transform.parent)
                    target = current;

            bool wasRemoved = currentTargets.Remove(target);

            if (wasRemoved)
                StartCoroutine(CheckForTargetWithSort());

            if (target.CompareTag("Temporary"))
                Destroy(target); 

            return wasRemoved;
        }
        return false;
    }

    public void RemoveTarget(int index) {
        if (index < currentTargets.Count)
            RemoveTarget(currentTargets[index]);
    }

    public void ClearTargets() {
        if (currentTargets != null) {
            foreach (GameObject target in currentTargets) {
                if (target != null && target.CompareTag("Temporary"))
                    Destroy(target); 
            }
            currentTargets.Clear();
        }
        else
            currentTargets = new List<GameObject>();
    }

    public IEnumerator PauseFor(float seconds) {
        pause = true;
        controller.SetTarget(gameObject.transform);
        yield return new WaitForSeconds(seconds);
        pause = false;
        CheckForTarget();
    }

    // sorting

    int SortByDistance(GameObject g1, GameObject g2) {
        if (g1 == null)
            return -1;

        if (g2 == null)
            return 1;

        float one = g1.DistanceBetweenSqr(gameObject);
        float two = g2.DistanceBetweenSqr(gameObject);

        return one.CompareTo(two);
    }

    // getters

    public List<GameObject> GetTargetList() {
        return currentTargets;
    }

    public GameObject GetCurrentTarget() {
        if (controller.target != null)
           return controller.target.gameObject;
        return null;
    }

    // setters

    public void SetAimTarget(GameObject aim) {
        if (aim != null)
            controller.aimTarget = aim.transform;
        else if (controller != null)
            controller.aimTarget = null;
    }
}
