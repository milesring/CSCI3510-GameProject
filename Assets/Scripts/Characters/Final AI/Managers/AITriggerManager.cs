using System;
using System.Collections.Generic;
using UnityEngine;

public class AITriggerManager : MonoBehaviour {

    public List<Trigger> triggers;
    public List<Trigger> resetableTriggers;

    public struct Trigger {
        public string name;
        public float radius;
        public float radSqr;
        public bool resetable;
        public SphereCollider collider;
        public Action<Collider> EnterFunc;
        public Action<Collider> ExitFunc;
    }

    public void Awake() {
        triggers = new List<Trigger>();
        resetableTriggers = new List<Trigger>();
    }

    private void Start() {

    }

    public void AddTrigger(string name, float radius, Action<Collider> enterAction, bool resetable = false) {
        Trigger trig = new Trigger();
        trig.name = name;
        trig.radius = radius;
        trig.EnterFunc = enterAction;
        trig.ExitFunc = NullFunc;
        trig.resetable = resetable;

        AddTrigger(trig);
    }
    public void AddTrigger(string name, float radius, Action<Collider> enterAction, Action<Collider> exitAction, bool resetable = false) {
        Trigger trig = new Trigger();
        trig.name = name;
        trig.radius = radius;
        trig.EnterFunc = enterAction;
        trig.ExitFunc = exitAction;
        trig.resetable = resetable;

        AddTrigger(trig);
    }

    public void AddTrigger(Trigger trig) {
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = trig.radius;
        collider.isTrigger = true;
        trig.collider = collider;
        trig.radSqr = trig.radius * trig.radius;

        triggers.Add(trig);
        triggers.Sort(SortByRadius);
        if (trig.resetable)
            resetableTriggers.Add(trig);
    }

    int SortByRadius(Trigger t1, Trigger t2) { // radius biggest to small
        return t2.radius.CompareTo(t1.radius);
    }

    // -------------------- \\

    public void SetName(string name) {
        this.name = name;
    }

    public void NullFunc(Collider other) {
        // Wasn't set, do nothing
    }

    public void ResetTriggers() {
        foreach (Trigger trigger in resetableTriggers) {
            Vector3 position = trigger.collider.gameObject.transform.position;
            Collider[] colliders = Physics.OverlapSphere(position, trigger.radius);
            foreach (Collider col in colliders) {
                trigger.EnterFunc(col);
            }
        }
    }

    // -------------------- \\

    private void OnTriggerEnter(Collider other) {
        int previous = 0;
        for (int x = 0; x < triggers.Count; x++) {
            if (triggers[x].radSqr > gameObject.DistanceBetweenSqr(other.ClosestPoint(this.gameObject.transform.position))) {
                previous = x;
            } else {
                triggers[previous].EnterFunc(other);
                break;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        foreach (Trigger trigger in triggers) { // find the largest (first) radius that is still smaller than the distance
            if (trigger.radSqr < gameObject.DistanceBetweenSqr(other.ClosestPoint(this.gameObject.transform.position))) {
                trigger.ExitFunc(other);
                break;
            }
        }
    }

}
