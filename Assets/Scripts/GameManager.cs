using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private ZombieController[] zombiePrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private AudioSource soundEffects;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip playerDeathSound;
    [SerializeField] private int killsToWin = 8;
    [SerializeField] private int suppliesToWin = 3;
    [SerializeField] private float gameTime = 120f;

    private readonly List<ZombieController> zombies = new List<ZombieController>();
    private int health = 100;
    private int kills;
    private int supplies;
    private float timeLeft;
    private float spawnTimer = 4f;
    private bool started;
    private bool paused;
    private bool won;
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;

    public bool IsFinished { get; private set; }
    public bool CanPlay { get { return started && !paused && !IsFinished; } }

    private int Wave
    {
        get
        {
            if (kills >= 6) return 3;
            if (kills >= 3) return 2;
            return 1;
        }
    }

    private void Start()
    {
        timeLeft = gameTime;
    }

    private void StartGame()
    {
        started = true;
        SpawnZombie();
        SpawnZombie();
    }

    private void Update()
    {
        if (started && !IsFinished && Input.GetKeyDown(KeyCode.P))
            paused = !paused;

        if (IsFinished)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            return;
        }

        if (!CanPlay) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            Finish(false);
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            int maxZombies = Wave == 1 ? 3 : Wave == 2 ? 4 : 5;
            if (kills < killsToWin && zombies.Count < maxZombies)
                SpawnZombie();
            spawnTimer = Wave == 1 ? 4f : Wave == 2 ? 3f : 2.5f;
        }
    }

    private void SpawnZombie()
    {
        int type = Random.Range(0, zombiePrefabs.Length);
        int point = Random.Range(0, spawnPoints.Length);
        ZombieController zombie = Instantiate(zombiePrefabs[type],
            spawnPoints[point].position, Quaternion.identity);
        zombie.Setup(player.transform, this);
        zombies.Add(zombie);
    }

    public void TakeDamage(int amount)
    {
        if (IsFinished) return;

        health = Mathf.Max(0, health - amount);
        soundEffects.PlayOneShot(hurtSound, 0.5f);
        if (health == 0)
        {
            soundEffects.PlayOneShot(playerDeathSound, 0.5f);
            Finish(false);
        }
    }

    public void ZombieKilled(ZombieController zombie)
    {
        zombies.Remove(zombie);
        kills++;
        CheckWin();
    }

    public void CollectSupply()
    {
        supplies++;
        health = Mathf.Min(100, health + 15);
        timeLeft += 5f;
        CheckWin();
    }

    private void CheckWin()
    {
        if (kills >= killsToWin && supplies >= suppliesToWin)
            Finish(true);
    }

    private void Finish(bool victory)
    {
        IsFinished = true;
        won = victory;
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label);
            hudStyle.fontSize = 26;
            hudStyle.fontStyle = FontStyle.Bold;
            hudStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(hudStyle);
            titleStyle.fontSize = 44;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 24;
        }

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        if (!started)
        {
            GUI.Box(new Rect(centerX - 320, centerY - 165, 640, 330), "");
            GUI.Label(new Rect(centerX - 300, centerY - 130, 600, 65),
                "SURVIVAL SHOOTER", titleStyle);
            GUI.Label(new Rect(centerX - 270, centerY - 55, 540, 45),
                "Defeat 8 zombies and collect 3 supplies.", hudStyle);
            GUI.Label(new Rect(centerX - 270, centerY - 10, 540, 45),
                "Supplies restore health and add 5 seconds.", hudStyle);
            if (GUI.Button(new Rect(centerX - 110, centerY + 65, 220, 60),
                "START GAME", buttonStyle))
                StartGame();
            return;
        }

        GUI.Box(new Rect(15, 15, 470, 205), "");
        GUI.Label(new Rect(30, 20, 400, 38), "HEALTH  " + health, hudStyle);
        GUI.Label(new Rect(30, 60, 440, 38), "ZOMBIES  " + kills + " / " + killsToWin, hudStyle);
        GUI.Label(new Rect(30, 100, 440, 38), "SUPPLIES  " + supplies + " / " + suppliesToWin, hudStyle);
        GUI.Label(new Rect(30, 140, 440, 38), "WAVE  " + Wave, hudStyle);
        GUI.Label(new Rect(30, 180, 440, 38), "TIME  " + Mathf.CeilToInt(timeLeft) + "s", hudStyle);
        GUI.Label(new Rect(15, Screen.height - 50, 950, 42),
            "WASD: move  |  Mouse: aim  |  Click/Space: shoot  |  P: pause", hudStyle);

        if (paused)
        {
            GUI.Box(new Rect(centerX - 220, centerY - 105, 440, 210), "");
            GUI.Label(new Rect(centerX - 180, centerY - 65, 360, 60),
                "PAUSED", titleStyle);
            if (GUI.Button(new Rect(centerX - 100, centerY + 25, 200, 55),
                "RESUME", buttonStyle))
                paused = false;
            return;
        }

        if (!IsFinished) return;

        GUI.Box(new Rect(centerX - 255, centerY - 115, 510, 230), "");
        GUI.Label(new Rect(centerX - 230, centerY - 75, 460, 65),
            won ? "YOU SURVIVED!" : "GAME OVER", titleStyle);
        GUI.Label(new Rect(centerX - 175, centerY - 5, 350, 42),
            "Press R to try again", hudStyle);
        if (GUI.Button(new Rect(centerX - 100, centerY + 55, 200, 55),
            "PLAY AGAIN", buttonStyle))
            Restart();
    }
}
