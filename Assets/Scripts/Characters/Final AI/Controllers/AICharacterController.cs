using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof (NavMeshAgent))]
[RequireComponent(typeof (AICharacter))]
public class AICharacterController : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
    public AICharacter character { get; private set; } // the character we are controlling
    public Transform target;
    public Transform aimTarget;
    bool fixingPath = false;

    public GameObject tempObject;
    public GameObject originalObject;

    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        agent = GetComponentInChildren<NavMeshAgent>();
        character = GetComponent<AICharacter>();

	    agent.updateRotation = false;
	    agent.updatePosition = true;
    }


    private void Update() {
		if (target != null)
			agent.SetDestination (target.position);

        if (agent.remainingDistance > agent.stoppingDistance)
            character.Move(agent.desiredVelocity, false, false);
        else {
            character.Move(Vector3.zero, false, false);
            if (aimTarget != null)
                RotateToFaceTarget();
        }

        StartCoroutine(FixPath());
    }

    private void RotateToFaceTarget() {
        float rotationSpeed = 5.0f;
        Quaternion targetRotation = Quaternion.LookRotation(aimTarget.position - transform.position);
        float str = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
    }

    public void RunInDirection(Vector3 direction) {
        character.Move(direction, false, false);
    }

    IEnumerator FixPath() {
        if (fixingPath == false && agent != null && target != null && !agent.CalculatePath(target.position, agent.path)) {
            fixingPath = true;

            // create temp target
            Destroy(tempObject);
            tempObject = new GameObject();
            tempObject.tag = "Temporary";
            tempObject.name = "Temporary AI Waypoint";
            tempObject.transform.position = this.target.position;

            // hold on to original target
            originalObject = target.gameObject;

            AIMovementManager movement = GetComponent<AIMovementManager>();

            Vector3 newPos = gameObject.DirectionToObject(originalObject);
            newPos.Normalize();
            yield return null;

            tempObject.transform.position = gameObject.transform.position += (new Vector3(newPos.x * 8, newPos.y * 2, newPos.z * 8));

            movement.RemoveTarget(0);
            movement.AddTarget(tempObject);
            yield return null;
        }
        yield return null;
        fixingPath = false;
    }

    public void SetTarget(Transform target, Vector3 offset = default(Vector3)) {
        this.target = target;
    }

    public Transform GetTarget() {
        return target.transform;
    }
}
