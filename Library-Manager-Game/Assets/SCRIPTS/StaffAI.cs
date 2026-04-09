using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StaffAI : MonoBehaviour
{
    // 'protected' means only this script and classes that inherit from it can access these
    protected NavMeshAgent agent;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Randomise priority so staff don't get stuck doing the "NavMesh Dance" with customers
        agent.avoidancePriority = Random.Range(1, 100);
    }

    // Universal movement command for all staff members.
    protected void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    // Universal check to see if a staff member has reached their current goal.
    protected bool HasReachedDestination()
    {
        if (agent.pathPending) return false;
        if (!agent.hasPath) return true; 

        // 1. Perfect mathematical arrival
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f) return true;

        // 2. The "Stuck" Check
        if (agent.remainingDistance <= 2.5f && agent.velocity.sqrMagnitude < 0.05f) 
        {
            return true;
        }
        
        return false;
    }
}