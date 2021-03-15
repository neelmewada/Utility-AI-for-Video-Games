# State Machine Utility AI for Video Games (Tank Demo)

A Tank Game project showcasing what Utility AI is and how to implement it in video games, using Unity Engine and C#

Downloadable demo available for macOS: [Download macOS Demo](https://github.com/neelmewada/Utility-AI-for-video-games/releases)

Link to a commercial iOS game that me and my business partner worked on, that implemented the Utility AI system: [Refill.io on App Store](https://apps.apple.com/us/app/refill-io/id1447779661)

If you want to take a look at all the C# scripts, then they are at the location ```Assets/Scripts/``` in this GitHub repository.

### Screenshot

![](https://i.ibb.co/MCPQWRv/Screenshot-2021-03-14-at-2-27-48-PM.png)

------

#### What is Utility AI?

Utility AI system is a state machine AI that determines which AI state to execute based on mathematical functions depending on factors like: player distance, health, score, etc.

The AI state with the largest score is then executed.

#### Why do you need Utility AI?

AI is the most important aspect of developing a fun and addictive video game. It can make or break your game. And it has to be the right blend.

And the most simplest way to make a video game AI is with just if-else statements like the simple example below:

```C#
if (playerDist < 5f) {
	Shoot();   // Shoot state
} else if (health < 30) {
	Flee();     // Flee state
} else {
	Patrol();  // Patrol through waypoints
}
```

You can definitely make it better by implementing a Finite State Machine, but that still will use basic conditions like these to determine which AI state to execute.

That's the problem that Utility AI solves. It uses mathematical calculations to calculate score of each viable state and executes the state with highest score value.

It's this score calculations that makes the AI feel like a real player, but still be predictable and feel deterministic. Those are the key ingredients to making great video game AI's.

## Implementation in Tanks Demo

If you want to take a deeper dive into the Utility AI specific code, then check out [BotState.cs](https://github.com/neelmewada/Utility-AI-for-video-games/blob/master/Assets/Scripts/AI/BotState.cs) and [BotController.cs](https://github.com/neelmewada/Utility-AI-for-video-games/blob/master/Assets/Scripts/AI/BotController.cs).

In my Tanks Demo, each AI tank can have 3 states. Each state is derived from an abstract class ```BotState```. It contains 3 abstract method that each child State classes have to override. Check out classes of the 3 states below:

#### 1) Patrol State:
In Patrol State, the AI tank will patrol through a set of waypoints. Below is the class that describes the Patrol State

```C#
public class BotPatrolState : BotState {
	// The score value for patrol state
    public override float CalculateScore(BotController bot) { return 10; }
	// Called once when Patrol State is activated
    public override void OnStateChanged(BotController bot, BotState previousState) {
        Vector3 wp = bot.GetRandomWaypoint();
        bot.SetPathDestination(wp); // Set a random waypoint as destination
        bot.StoppingDistance = bot.waypointSkipDist;
    }
	// Update logic for the patrol state, i.e. the code that runs every frame
    public override void UpdateState(BotController bot) {
        if (bot.IsPathEmpty()) { // If waypoint reached already
            Vector3 newWaypoint = bot.GetRandomWaypoint();
            bot.SetPathDestination(newWaypoint);
        }
    }
}
```

#### 2) Chase State:
In Chase state, the AI tank will chase the player tank and also shoot it if it's in the shooting range.

```C#
public class BotChaseState : BotState {
    private Vector3 m_LastPlayerPos;

    public override float CalculateScore(BotController bot) {
        var player = GameManager.Instance.Player;
        if (player == null || player.IsDead) return 0;

        float dist = Vector3.Distance(bot.transform.position, player.transform.position);
        float score = dist > bot.spotRadius ? 0 : 20;

        return score;
    }

    public override void OnStateChanged(BotController bot, BotState previousState) {
        bot.StoppingDistance = 10;
    }

    public override void UpdateState(BotController bot) {
        var player = GameManager.Instance.Player;
        if (player == null || player.IsDead) return;

        if (Vector3.Distance(player.transform.position, m_LastPlayerPos) > 5f) {
            bot.SetPathDestination(player.transform.position);
        }

        m_LastPlayerPos = player.transform.position;
        bot.ShootDirection = player.transform.position - bot.transform.position;

        // Square distance calculation is faster and efficient
        if (bot.ShootDirection.sqrMagnitude < bot.maxShootDist * bot.maxShootDist) {
            bot.Shoot();
        }
    }
}
```

#### 3) Flee State:
In Flee state, the AI tank will flee away from the player when it's health is low.

```C#
public class BotFleeState : BotState {
    public override float CalculateScore(BotController bot) {
        return (40 - bot.Health) * 4f;
    }

    public override void OnStateChanged(BotController bot, BotState previousState) {
        bot.StoppingDistance = bot.waypointSkipDist;
        Debug.Log("Flee State: " + bot.gameObject.name, bot.gameObject);
    }

    public override void UpdateState(BotController bot) {
        var player = GameManager.Instance.Player;
        if (player == null || player.IsDead) return;

        bot.ResetWaypoints(); // We'll be using all the waypoints
        float minDotValue = float.MaxValue;
        Vector3 targetWP = bot.transform.position;
		
		// Get a waypoint that is in opposite direction to that of player
        bot.ForEachWaypoint(wp =>
        {
            float dot = Vector3.Dot(wp - bot.transform.position, player.transform.position - bot.transform.position);
            if (dot < minDotValue) {
                minDotValue = dot;
                targetWP = wp;
            }
        });
        bot.SetPathDestination(targetWP);
        bot.ShootDirection = player.transform.position - bot.transform.position;

        // Square distance calculation is faster and efficient
        if (bot.ShootDirection.sqrMagnitude < bot.maxShootDist * bot.maxShootDist) {
            bot.Shoot();
        }
    }
}
```


### Conclusion
------

So there you go. This is the simplest example on how to implement Utility AI system with a Finite State Machine for video games.

If you want to see a commercial game project I worked on that implemented the Utility AI system, then check out our [Refill.io game on AppStore](https://apps.apple.com/us/app/refill-io/id1447779661).


