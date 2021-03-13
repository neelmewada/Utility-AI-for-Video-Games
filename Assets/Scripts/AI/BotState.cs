using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Describes the states of the Bot State Machine
/// </summary>
public abstract class BotState
{

    public abstract float CalculateScore(BotController bot);

    /// <summary>
    /// Called when state is changed from previousState to this state
    /// </summary>
    public abstract void OnStateChanged(BotController bot, BotState previousState);

    /// <summary>
    /// Update logic for current state
    /// </summary>
    public abstract void UpdateState(BotController bot);
}

/// <summary>
/// Patrol State: Bot patrols thru the waypoints
/// </summary>
public class BotPatrolState : BotState
{

    public override float CalculateScore(BotController bot)
    {
        return 10;
    }

    public override void OnStateChanged(BotController bot, BotState previousState)
    {
        Vector3 wp = bot.GetRandomWaypoint();

        bot.SetPathDestination(wp); // Set a random waypoint as destination
        bot.StoppingDistance = bot.waypointSkipDist;
    }

    public override void UpdateState(BotController bot)
    {
        if (bot.IsPathEmpty()) // if waypoint is reached or path is empty, reset to new waypoint
        {
            Vector3 newWaypoint = bot.GetRandomWaypoint();

            bot.SetPathDestination(newWaypoint);
        }
    }
}

/// <summary>
/// Chase State: Bot chases the player
/// </summary>
public class BotChaseState : BotState
{
    private Vector3 m_LastPlayerPos;

    public override float CalculateScore(BotController bot)
    {
        var player = GameManager.Instance.Player;
        if (player == null || player.IsDead) return 0;

        float dist = Vector3.Distance(bot.transform.position, player.transform.position);
        float score = dist > bot.spotRadius ? 0 : 20;

        return score;
    }

    public override void OnStateChanged(BotController bot, BotState previousState)
    {
        bot.StoppingDistance = 10;
    }

    public override void UpdateState(BotController bot)
    {
        var player = GameManager.Instance.Player;
        if (player == null || player.IsDead) return;


        if (Vector3.Distance(player.transform.position, m_LastPlayerPos) > 5f)
        {
            bot.SetPathDestination(player.transform.position);
        }

        m_LastPlayerPos = player.transform.position;

        bot.ShootDirection = player.transform.position - bot.transform.position;

        // Square distance calculation is faster and efficient
        if (bot.ShootDirection.sqrMagnitude < bot.maxShootDist * bot.maxShootDist)
        {
            bot.Shoot();
        }
    }
}

/// <summary>
/// Flee State: Bot flees from the player
/// </summary>
public class BotFleeState : BotState
{
    public override float CalculateScore(BotController bot)
    {
        return (30 - bot.Health) * 2f;
    }

    public override void OnStateChanged(BotController bot, BotState previousState)
    {
        bot.StoppingDistance = bot.waypointSkipDist;
    }

    public override void UpdateState(BotController bot)
    {
        var player = GameManager.Instance.Player;
        if (player == null || player.IsDead) return;

        bot.ResetWaypoints(); // We'll be using all the waypoints
        float minDotValue = float.MaxValue;
        Vector3 targetWP = bot.transform.position;

        bot.ForEachWaypoint(wp => // Get a waypoint that is in opposite direction to that of player
        {
            float dot = Vector3.Dot(wp - bot.transform.position, player.transform.position - bot.transform.position);
            if (dot < minDotValue)
            {
                minDotValue = dot;
                targetWP = wp;
            }
        });

        bot.SetPathDestination(targetWP);
    }
}
