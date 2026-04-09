using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StaffAI : MonoBehaviour
{
    protected NavMeshAgent agent;

    [Header("RPG Stats")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5; // How many tasks to reach level 2

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = Random.Range(1, 100);
    }

    // Call this whenever the staff member completes a task.
    public void GainXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    protected virtual void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel; // Carry over excess XP
        
        // Make the next level 50% harder to reach
        xpToNextLevel = Mathf.FloorToInt(xpToNextLevel * 1.5f); 
        
        Debug.Log($"{gameObject.name} leveled up to Level {currentLevel}!");
        
        // Optional: Spawn a particle effect or popup text here!
    }

    protected void MoveTo(Vector3 destination) { agent.SetDestination(destination); }

    protected bool HasReachedDestination()
    {
        if (agent.pathPending) return false;
        if (!agent.hasPath) return true; 
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f) return true;
        if (agent.remainingDistance <= 2.5f && agent.velocity.sqrMagnitude < 0.05f) return true;
        return false;
    }
}