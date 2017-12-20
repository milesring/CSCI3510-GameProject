using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;


[RequireComponent(typeof(AITriggerManager))]
[RequireComponent(typeof(AIInventoryManager))]
[RequireComponent(typeof(AIMovementManager))]
[RequireComponent(typeof(AIThreat))]
// TODO

public class EnemyAI : MonoBehaviour {

    private AIMovementManager movement;
    private AIInventoryManager inventory;
    private AIWeaponManager weaponManager;
    public AITriggerManager triggers;

    public List<GameObject> placesToLoot;
    public List<GameObject> buildingsGLOBAL; // TODO remove, make global
    public List<AIThreat> threatsGLOBAL;
    public List<AIThreat> threats;
    public GameObject aimTarget; // TODO remove, put in weapon manager

    public Status status;
    public int lootLevel;

    private int aggression;
    private int skill;

    public enum Goals {
        LOOT, // when ai needs loot / health
        HUNT, // when ai has full loot / health
        STALK, // when ai has knowedge of player but doesn't engage
        ATTACK, // when ai engages a player
        HIDE, // when ai hides from a player
        FLEE // when ai runs from a threat
    }

    private InteractableStructure lastBuildingEntered;
    private List<GameObject> positionOptions;

    private Goals priorGoal;
    public Goals _goal;
    public Goals goal {
        get { return _goal; }
        set {
            priorGoal = _goal;
            _goal = value;
            if (_goal != priorGoal)
                OnGoalUpdated();
        }
    }

    /* Main Functions */

    void Start () {
        UnityEngine.Random.InitState((int)(System.DateTime.Now.Ticks));

        triggers = gameObject.GetComponent<AITriggerManager>(); // must be initialized first
        movement = gameObject.GetComponent<AIMovementManager>();
        inventory = gameObject.GetComponent<AIInventoryManager>();
        weaponManager = gameObject.GetComponent<AIWeaponManager>();

        BuildingSpawner buildingSpawner = GameObject.Find("BuildingSpawner").GetComponent<BuildingSpawner>();
        buildingsGLOBAL = buildingSpawner.buildingLocations;
        placesToLoot = new List<GameObject>(buildingsGLOBAL);

        CharacterSpawner charSpawner = GameObject.Find("CharacterSpawner").GetComponent<CharacterSpawner>();
        threatsGLOBAL = charSpawner.threats;
        //threats = new List<AIThreat>(threatsGLOBAL);

        status = gameObject.AddComponent<Status>();
        status.health = 100;
        status.maxHealth = 100;
        status.armor = 0;
        status.maxArmor = 100;

        lootLevel = 0;

        skill = (int)(UnityEngine.Random.value * 8 + 2);
        aggression = (int)(UnityEngine.Random.value * 9 + 1);

        positionOptions = new List<GameObject>();

        StartCoroutine(AILoop());
    }

    IEnumerator AILoop() {
        while (status.isDead == false) {
            threats = new List<AIThreat>(threatsGLOBAL);
            threats.Sort(SortThreatsByDistanceFromSelf);
            yield return null;
            int count = threats.Count;
            threats.RemoveRange(count / 2 + (count % 2 == 0 ? 0 : 1), count / 2);

            AIThreat target = GetClosestThreat();
            yield return null;
            float dist = gameObject.DistanceBetweenSqr(target.gameObject);

            if (status.health < 20) {
                SetAimTarget(null);
                goal = Goals.HIDE;
            } else if ((lootLevel < 8 || inventory.GetAmmoInventorySize() <= 2) && dist > Mathf.Pow(5, 2)) {
                SetAimTarget(null);
                goal = Goals.LOOT;
                OnGoalUpdated();
            } else if (CheckForSightOnTarget(target.gameObject)) {
                if (aggression < 5 && dist < Mathf.Pow(10, 2)) {
                    SetAimTarget(null);
                    goal = Goals.FLEE;
                } else {
                    SetAimTarget(target.gameObject);
                    goal = Goals.ATTACK;
                }

            } else {
                SetAimTarget(target.gameObject);
                goal = Goals.STALK;
            }
            yield return new WaitForSeconds(3);
        }
    }

    AIThreat GetClosestThreat() {
        AIThreat closest = threats[0];

        foreach (AIThreat threat in threats) {
            if (threat.gameObject != gameObject) // if its not me
                return threat;
        }
        Debug.Log("SHOULD NEVER HAPPEN");
        return null; // should never happen...
    }

    void SetAimTarget(GameObject target) {
        aimTarget = target;
        movement.SetAimTarget(target);
    }

    void OnGoalUpdated() {
        switch (goal) {
            case Goals.LOOT:
                movement.ClearTargets();
                movement.AddTargets(placesToLoot);
                break;
            case Goals.HUNT:
                movement.ClearTargets();
                movement.AddTargets(buildingsGLOBAL);
                break;
            case Goals.STALK:
                movement.ClearTargets();
                movement.AddTargets(buildingsGLOBAL);
                break;
            case Goals.ATTACK:
                StartCoroutine(AttackTarget());
                break;
            case Goals.HIDE:
                movement.ClearTargets();
                movement.AddTargets(buildingsGLOBAL);
                break;
            case Goals.FLEE:
                aimTarget = null;
                StartCoroutine(FleeFromThreats());
                break;
        }

        if (lastBuildingEntered != null && lastBuildingEntered.CheckForAIInBuilding(this.gameObject))
            OnEnterBuilding(lastBuildingEntered);
    }

    public void TargetReached(GameObject target) {
        switch (goal) {
            case Goals.LOOT:
                movement.NextTarget();
                break;
            case Goals.HUNT:
                movement.NextTarget();
                break;
            case Goals.STALK:
                StartCoroutine(CheckForSight());
                break;
            case Goals.ATTACK:

                break;
            case Goals.HIDE:
                inventory.UseMedical();
                break;
            case Goals.FLEE:

                break;
        }
    }

    public void OnEnterBuilding(InteractableStructure building) {
        lastBuildingEntered = building;

        switch (goal) {
            case Goals.LOOT:
                movement.AddTargets(building.lootingWaypoints);
                break;
            case Goals.HUNT:
                movement.AddTargets(building.lootingWaypoints);
                break;
            case Goals.STALK:
                positionOptions.Clear();
                positionOptions.AddRange(building.attackCoverWaypoints);
                FindPlaceToStalk();
                break;
            case Goals.ATTACK:

                break;
            case Goals.HIDE:
                positionOptions.Clear();
                positionOptions.AddRange(building.defensiveCoverWaypoints);
                FindPlaceToHide();
                break;
            case Goals.FLEE:

                break;
        }
    }

    /* Positioning Functions */

    void FindPlaceToHide() {
        GameObject bestOption = default(GameObject);
        float bestOptionFloat = float.MaxValue;

        foreach (GameObject posOption in positionOptions) {
            Waypoint point = posOption.GetComponent<Waypoint>();
            GameObject option = point.GetViewpoint(); // Object that represents headlevel at the waypoint

            float counter = 0;
            float sqrDistance = gameObject.DistanceBetweenSqr(option);

            foreach (AIThreat threat in threats) {
                bool seen = CheckForLineOfSight(point.gameObject, threat.gameObject, 50);

                if (seen)
                        counter++;
            }

            float optionDetails = ((counter * counter) + 0.1f) * (sqrDistance < 4 ? 4 : sqrDistance) * (point.indoors ? 1 : 1.5f);
            if (bestOptionFloat > optionDetails) {
                bestOption = posOption;
                bestOptionFloat = optionDetails;
            }
        }

        movement.SetManualTarget(bestOption);
    }

    void FindPlaceToStalk() {
        GameObject bestOption = default(GameObject);
        float bestOptionFloat = float.MaxValue;
        bool targetVisible;

        foreach (GameObject posOption in positionOptions) {
            Waypoint point = posOption.GetComponent<Waypoint>();
            GameObject option = point.GetViewpoint(); // Object that represents headlevel at the waypoint
            targetVisible = false;

            float counter = 0;
            float sqrDistance = gameObject.DistanceBetweenSqr(option);

            foreach (AIThreat threat in threats) {
                bool seen = CheckForLineOfSight(point.gameObject, threat.gameObject, 50);

                if (seen) {
                    if (threat.gameObject == aimTarget)
                        targetVisible = true;
                    else
                        counter++;
                }
            }

            float optionDetails = ((counter * counter) + 0.1f) * (sqrDistance < 8 ? 8 : sqrDistance) * (point.indoors ? 1 : 1.5f);
            if (targetVisible && bestOptionFloat > optionDetails && posOption != movement.GetCurrentTarget()) {
                bestOption = posOption;
                bestOptionFloat = optionDetails;
            }
        }

        movement.SetManualTarget(bestOption);
    }

    IEnumerator CheckForSight() {
        while (goal == Goals.STALK) {
            bool sight = CheckForSightOnTarget(aimTarget);
            yield return null;
            while (sight && goal == Goals.STALK) {

                float sqrRange = gameObject.DistanceBetweenSqr(aimTarget);
                yield return null;

                if (weaponManager.SetWeaponForRangeSqr(sqrRange) && CheckForSightOnTarget(aimTarget)) { // shoot target
                    movement.SetManualTarget(this.gameObject);
                    if (!weaponManager.IsReloadingWeapon())
                        weaponManager.FireAt(aimTarget);
                    yield return new WaitForSeconds(0.2f);
                }

                sight = CheckForSightOnTarget(aimTarget);
                yield return new WaitForSeconds(1);
            }

            FindPlaceToStalk();
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator AttackTarget() {
        while (goal == Goals.ATTACK) {
            float sqrRange = gameObject.DistanceBetweenSqr(aimTarget);
            yield return null;

            if (weaponManager.SetWeaponForRangeSqr(sqrRange) && CheckForSightOnTarget(aimTarget)) {
                movement.SetManualTarget(this.gameObject);
                if (!weaponManager.IsReloadingWeapon())
                weaponManager.FireAt(aimTarget);
                yield return new WaitForSeconds(0.2f);
            } else if (sqrRange <= 1) { // is super close and has no guns...
                if (weaponManager.GetEquippedWeapon() != null)
                    weaponManager.SelectWeapon();
                movement.SetManualTarget(this.gameObject);
                weaponManager.FireAt(aimTarget);
                yield return new WaitForSeconds(0.2f);
            } else {
                movement.SetManualTarget(aimTarget);
            }

            yield return null;
        }
    }

    IEnumerator FleeFromThreats() {
        GameObject fleeTarget = new GameObject();
        fleeTarget.transform.position = gameObject.transform.position;
        movement.SetManualTarget(fleeTarget);

        while (goal == Goals.FLEE) {
            Vector3 direction = new Vector3();

            foreach (AIThreat threat in threats) {
                direction -= gameObject.DirectionToObject(threat.gameObject).normalized;
                yield return null;
            }

            direction.Normalize();
            yield return null;
            fleeTarget.transform.position = gameObject.transform.position + (new Vector3(direction.x * 8, direction.y * 2, direction.z * 8));
            yield return new WaitForSeconds(1);
        }
        Destroy(fleeTarget);
    }

    bool CheckForSightOnTarget(GameObject target) {
        Debug.Log("Me: " + gameObject);
        Debug.Log("Them: " + target);
        GameObject originalTarg = target;
        AIThreat threat = target.GetComponent<AIThreat>();
        if (threat != null)
            target = threat.head;

        float viewDistance = 50;
        GameObject aim = weaponManager.GetAimObject();
        aim.transform.LookAt(target.transform);

        RaycastHit[] LOS = Physics.RaycastAll(aim.transform.position, aim.transform.forward, viewDistance);
        Array.Sort(LOS, SortByDistance);

        GameObject seen = null;
        for (int x = 0; x < LOS.Length; x++)
            if (LOS[x].collider.isTrigger == false && LOS[x].collider.gameObject != gameObject) {
                seen = LOS[x].collider.gameObject;
                break;
            }

        return seen == originalTarg;
    }

    bool CheckForLineOfSight(GameObject one, GameObject two, float viewDistance = float.MaxValue, bool debug = false) {
        GameObject firstComp;
        GameObject secondComp;

        AIThreat threatOne = one.GetComponent<AIThreat>();
        AIThreat threatTwo = two.GetComponent<AIThreat>();
        Waypoint wayOne = one.GetComponent<Waypoint>();
        Waypoint wayTwo = two.GetComponent<Waypoint>();

        if (threatOne != null)
            firstComp = threatOne.head;
        else if (wayOne != null)
            firstComp = wayOne.GetViewpoint();
        else
            firstComp = one;

        if (threatTwo != null)
            secondComp = threatTwo.head;
        else if (wayTwo != null)
            secondComp = wayTwo.GetViewpoint();
        else
            secondComp = two;

        GameObject aim = new GameObject();
        aim.transform.position = firstComp.transform.position;
        aim.transform.LookAt(secondComp.transform);

        RaycastHit[] LOS = Physics.RaycastAll(aim.transform.position, aim.transform.forward, viewDistance);
        Array.Sort(LOS, SortByDistance);

        GameObject seen = null;
        for (int x = 0; x < LOS.Length; x++)
            if ((LOS[x].collider.isTrigger == false || LOS[x].collider.gameObject.CompareTag("Waypoint")) && LOS[x].collider.gameObject != gameObject) {
                seen = LOS[x].collider.gameObject;
                if (debug)
                    DrawSomeStuff(firstComp, seen, LOS[x]);
                break;
            }

        Destroy(aim);
        return seen == two;
    }

    int SortByDistance(RaycastHit r1, RaycastHit r2) { // radius biggest to small
        return r1.distance.CompareTo(r2.distance);
    }

    int SortByDistanceFromSelf(GameObject g1, GameObject g2) { // radius biggest to small
        return g1.DistanceBetween(gameObject).CompareTo(g2.DistanceBetween(gameObject));
    }

    int SortThreatsByDistanceFromSelf(AIThreat g1, AIThreat g2) { // radius biggest to small
        return g1.gameObject.DistanceBetween(gameObject).CompareTo(g2.gameObject.DistanceBetween(gameObject));
    }

    public void ResetTriggers() {
        triggers.ResetTriggers();
    }

    void DrawSomeStuff(GameObject one, GameObject two, RaycastHit LOS) {
        float length = one.DistanceBetween(two);
        Vector3 newTemp = two.transform.forward.normalized;
        newTemp *= LOS.distance;
        Color col = Color.green;
        Debug.DrawRay(two.transform.position, newTemp, col, 10, false);
    }

}
