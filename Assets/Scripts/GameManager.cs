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
    [SerializeField] private float gameTime = 90f;

    private readonly List<ZombieController> zombies = new List<ZombieController>();
    private int health = 100;
    private int kills;
    private float timeLeft;
    private float spawnTimer = 4f;
    private bool won;
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;

    public bool IsFinished { get; private set; }

    private void Start()
    {
        timeLeft = gameTime;
        SpawnZombie();
        SpawnZombie();
    }

    private void Update()
    {
        if (IsFinished)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            Finish(false);
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && zombies.Count < 4)
        {
            SpawnZombie();
            spawnTimer = 4f;
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
        if (kills >= killsToWin) Finish(true);
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
        }

        GUI.Box(new Rect(15, 15, 430, 140), "");
        GUI.Label(new Rect(30, 20, 400, 38), "HEALTH  " + health, hudStyle);
        GUI.Label(new Rect(30, 60, 400, 38), "ZOMBIES  " + kills + " / " + killsToWin, hudStyle);
        GUI.Label(new Rect(30, 100, 400, 38), "TIME  " + Mathf.CeilToInt(timeLeft) + "s", hudStyle);
        GUI.Label(new Rect(15, Screen.height - 50, 950, 42),
            "WASD: move  |  Mouse: aim  |  Click or Space: shoot", hudStyle);

        if (!IsFinished) return;

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        GUI.Box(new Rect(centerX - 255, centerY - 115, 510, 230), "");
        GUI.Label(new Rect(centerX - 230, centerY - 75, 460, 65),
            won ? "YOU SURVIVED!" : "GAME OVER", titleStyle);
        GUI.Label(new Rect(centerX - 175, centerY - 5, 350, 42),
            "Press R to try again", hudStyle);
        if (GUI.Button(new Rect(centerX - 90, centerY + 55, 180, 42), "PLAY AGAIN"))
            Restart();
    }
}
