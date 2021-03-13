using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// The class that controls the AI tanks. It implements a Finite Statemachine with the Utility AI System
/// </summary>
public class BotController : TankController
{
    [Header("AI")]
    public NavMeshAgent agent;
    public float spotRadius = 10; // the max distance at which bot can spot the player
    public float waypointSkipDist = 3;
    public float maxShootDist = 20;
    public bool debug = false;

    private List<Vector3> m_WaypointSelector = new List<Vector3>();
    private List<Vector3> m_CurrentPath = new List<Vector3>();
    private List<BotState> m_AllStates = new List<BotState>();

    private TankController m_EnemyTarget;
    private bool m_CanMove = true;

    public TankController EnemyTarget { get => m_EnemyTarget; }
    public BotState CurrentState { get; private set; }
    public BotState PreviousState { get; private set; }
    public float StoppingDistance { get; set; }


    protected override void Awake()
    {
        base.Awake();
        ResetWaypoints();

        m_AllStates.Add(new BotPatrolState()); // Patrols thru waypoints
        m_AllStates.Add(new BotChaseState()); // Chases the player tank
        m_AllStates.Add(new BotFleeState()); // Flee from the player tank

        CurrentState = m_AllStates[0];
    }

    protected override void Update()
    {
        base.Update();

        if (!CanUpdate()) // If Update logic is disabled right now
        {
            MoveVector = Vector3.zero;
            return;
        }

        UpdateStateMachineScores(); // Update Utility scores

        CurrentState.UpdateState(this); // Call Update logic for current state

        if (!m_CanMove || m_CurrentPath.Count == 0)
        {
            MoveVector = Vector3.zero;
            return;
        }

        if (Vector3.Distance(transform.position, m_CurrentPath[0]) < StoppingDistance)
        {
            m_CurrentPath.RemoveAt(0);
        }

        if (m_CurrentPath.Count == 0)
        {
            MoveVector = Vector3.zero;
            return;
        }

        Vector3 dir = m_CurrentPath[0] - transform.position;
        MoveVector = dir.normalized;
    }

    /// <summary>
    /// This function implements the Utility AI by calculating scores of each viable state and sets the current state
    /// </summary>
    protected void UpdateStateMachineScores()
    {
        var player = GameManager.Instance.Player;

        float[] stateScores = new float[m_AllStates.Count]; // [0] = patrol, [1] = chase, [2] = shoot, ...
        int highScoringState = 0;
        float highestRunningScore = 0;

        for (int i = 0; i < m_AllStates.Count; i++)
        {
            stateScores[i] = m_AllStates[i].CalculateScore(this);
            if (stateScores[i] > highestRunningScore)
            {
                highestRunningScore = stateScores[i];
                highScoringState = i;
            }
        }

        if (highestRunningScore == -1) return; // -1 means state isn't available right now

        BotState state = m_AllStates[highScoringState];
        if (CurrentState != state) // if state needs to be changed
        {
            PreviousState = CurrentState;
            CurrentState = state;
            CurrentState.OnStateChanged(this, PreviousState);
        }
    }

    // Resets the waypoint selector list. This list is used to cycle through waypoints
    public void ResetWaypoints()
    {
        if (GameManager.Instance.waypointsParent.childCount == m_WaypointSelector.Count)
            return;

        m_WaypointSelector.Clear();

        for (int i = 0; i < GameManager.Instance.waypointsParent.childCount; i++)
        {
            m_WaypointSelector.Add(GameManager.Instance.waypointsParent.GetChild(i).position);
        }
    }

    public Vector3 GetRandomWaypoint()
    {
        if (m_WaypointSelector.Count == 0)
            ResetWaypoints();

        int wpIndex = Random.Range(0, m_WaypointSelector.Count);
        Vector3 waypoint = m_WaypointSelector[wpIndex];
        m_WaypointSelector.RemoveAt(wpIndex);

        return waypoint;
    }

    public bool SetPathDestination(Vector3 destination)
    {
        m_CurrentPath.Clear();

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, destination, -1, path))
        {
            Debug.LogError("FATAL ERROR: The destination at position " + destination + " is NOT reachable!");
            return false;
        }

        for (int i = 0; i < path.corners.Length; i++)
        {
            if (Vector3.Distance(path.corners[i], transform.position) > 3f)
                m_CurrentPath.Add(path.corners[i]);
        }

        return m_CurrentPath.Count > 0;
    }

    public bool IsPathEmpty()
    {
        return m_CurrentPath.Count == 0;
    }

    public void ForEachWaypoint(System.Action<Vector3> wpFunction)
    {
        for (int i = 0; i < m_WaypointSelector.Count; i++)
        {
            wpFunction?.Invoke(m_WaypointSelector[i]);
        }
    }
}
