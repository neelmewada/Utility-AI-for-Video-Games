using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Single global instance of this class

    public new CameraController camera;
    public Transform tankSpawnPointsParent;
    public Transform collectableSpawnPointsParent;
    public Transform waypointsParent;
    public PlayerController playerPrefab;
    public BotController botPrefab;

    [Header("Gameplay")]
    public int botsCount = 4;

    [Header("UI")]
    public Text messageDisplay;

    public bool HasGameStarted { get; private set; }
    public bool HasGameFinished { get; private set; }

    public PlayerController Player { get; private set; }
    public List<BotController> Bots { get; private set; } = new List<BotController>();

    private List<Transform> m_TankSpawns = new List<Transform>();
    private List<Transform> m_CollectableSpawns = new List<Transform>();


    private void Awake()
    {
        Instance = this;
        messageDisplay.text = "Use WASD to move\nAnd, Click to shoot\n\nPress Enter To Start!";
    }

    private void Start()
    {
        InitializeLevel();
    }

    private void Update()
    {
        if (!HasGameStarted && Input.GetKeyDown(KeyCode.Return)) // Start the game when user presses Enter key
        {
            HasGameStarted = true;
            messageDisplay.text = ""; // Clear the UI text
        }

        if (!HasGameStarted)
            return;

        if (Player == null || Player.IsDead)
        {
            HasGameFinished = true;
            messageDisplay.text = "Game Over!\nPress Enter to start again...";
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void OnBotDead(BotController bot)
    {
        Bots.Remove(bot);
    }

    /// <summary>
    /// Gets the spawn points which are children of "tankSpawnPointsParent" gameobject
    /// </summary>
    private void ResetTankSpawns()
    {
        m_TankSpawns.Clear();

        for (int i = 0; i < tankSpawnPointsParent.childCount; i++) // Get all the child transforms and add them as spawn points
        {
            m_TankSpawns.Add(tankSpawnPointsParent.GetChild(i));
        }
    }

    /// <summary>
    /// Spawns the player and AI tanks and initializes the level
    /// </summary>
    private void InitializeLevel()
    {
        ResetTankSpawns(); // Get all tank spawn points

        botsCount = Mathf.Min(botsCount, m_TankSpawns.Count - 1); // 1 spawn point is occupied by the player

        int playerSPIndex = Random.Range(0, m_TankSpawns.Count);
        Player = Instantiate(playerPrefab.gameObject, m_TankSpawns[playerSPIndex].position, Quaternion.identity).GetComponent<PlayerController>();
        m_TankSpawns.RemoveAt(playerSPIndex);

        camera.target = Player.transform;

        for (int i = 0; i < botsCount; i++)
        {
            int botSPIndex = Random.Range(0, m_TankSpawns.Count);
            var bot = Instantiate(botPrefab.gameObject, m_TankSpawns[botSPIndex].position, Quaternion.identity).GetComponent<BotController>();
            m_TankSpawns.RemoveAt(botSPIndex);

            Bots.Add(bot);
        }
    }
}
