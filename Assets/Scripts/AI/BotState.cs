using UnityEngine;
using System.Collections;
using System.Collections.Generic;


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

public class BotPatrolState : BotState
{

    public override float CalculateScore(BotController bot)
    {
        return 10;
    }

    public override void OnStateChanged(BotController bot, BotState previousState)
    {
        Vector3 wp = bot.GetRandomWaypoint();

        bot.SetPathDestination(wp);
        bot.StoppingDistance = bot.waypointSkipDist;
    }

    public override void UpdateState(BotController bot)
    {
        if (bot.IsPathEmpty()) // if waypoint reached, reset to new waypoint
        {
            Vector3 newWaypoint = bot.GetRandomWaypoint();

            bot.SetPathDestination(newWaypoint);
        }
    }
}

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


        if (Vector3.Distance(player.transform.position, bot.transform.position) > 10f)
        {
            bot.SetPathDestination(player.transform.position);
        }

        m_LastPlayerPos = player.transform.position;
    }
}
