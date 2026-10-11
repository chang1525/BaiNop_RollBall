using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SurvivalShooterGame : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusic;

    private sealed class Enemy
    {
        public GameObject root;
        public Collider collider;
        public float speed;
        public float nextAttack;
        public bool bunny;
    }

    private readonly List<Enemy> enemies = new List<Enemy>();
    private Camera gameCamera;
    private GameObject player;
    private CharacterController playerController;
    private AudioSource soundEffects;
    private AudioClip gunShot, playerHurt, playerDeath, bearDeath, bunnyDeath;
    private Material shotMaterial;
    private float timeLeft = 90f;
    private float nextSpawn = 4f;
    private float nextShot;
    private int health = 100;
    private int kills;
    private bool finished, won;
    private GUIStyle hudStyle, titleStyle;

    private void Start()
    {
        BuildWorld();
        gunShot = LoadSound("Player GunShot");
        playerHurt = LoadSound("Player Hurt");
        playerDeath = LoadSound("Player Death");
        bearDeath = LoadSound("ZomBear Death");
        bunnyDeath = LoadSound("ZomBunny Death");
        soundEffects = gameObject.AddComponent<AudioSource>();
        if (backgroundMusic != null)
        {
            AudioSource music = gameObject.AddComponent<AudioSource>();
            music.clip = backgroundMusic;
            music.loop = true;
            music.volume = 0.2f;
            music.Play();
        }
        SpawnEnemy();
        SpawnEnemy();
    }

    private void Update()
    {
        if (finished)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            return;
        }

        MoveAndAim();
        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && Time.time >= nextShot) Shoot();

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = enemies[i];
            Vector3 towardPlayer = player.transform.position - enemy.root.transform.position;
            towardPlayer.y = 0f;
            float distance = towardPlayer.magnitude;
            if (distance > 1.35f)
                enemy.root.transform.position += towardPlayer.normalized * enemy.speed * Time.deltaTime;
            if (distance > 0.01f)
                enemy.root.transform.rotation = Quaternion.LookRotation(towardPlayer);
            if (distance < 1.65f && Time.time >= enemy.nextAttack)
            {
                enemy.nextAttack = Time.time + 2f;
                health = Mathf.Max(0, health - 3);
                PlaySound(playerHurt);
                if (health == 0)
                {
                    PlaySound(playerDeath);
                    Finish(false);
                    return;
                }
            }
        }

        nextSpawn -= Time.deltaTime;
        if (nextSpawn <= 0f && enemies.Count < 4)
        {
            SpawnEnemy();
            nextSpawn = 4f;
        }
        timeLeft = Mathf.Max(0f, timeLeft - Time.deltaTime);
        if (timeLeft <= 0f) Finish(false);
    }

    private void MoveAndAim()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        playerController.Move(Vector3.ClampMagnitude(input, 1f) * (6f * Time.deltaTime));
        Ray mouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(mouseRay, out float distance))
        {
            Vector3 aim = mouseRay.GetPoint(distance) - player.transform.position;
            aim.y = 0f;
            if (aim.sqrMagnitude > 0.1f)
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation,
                    Quaternion.LookRotation(aim), 15f * Time.deltaTime);
        }
    }

    private void Shoot()
    {
        nextShot = Time.time + 0.28f;
        PlaySound(gunShot);
        Vector3 start = player.transform.position + Vector3.up * 1.1f + player.transform.forward * 0.65f;
        Vector3 end = start + player.transform.forward * 25f;
        if (Physics.Raycast(start, player.transform.forward, out RaycastHit hit, 25f))
        {
            end = hit.point;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (hit.collider != enemies[i].collider) continue;
                PlaySound(enemies[i].bunny ? bunnyDeath : bearDeath);
                Destroy(enemies[i].root);
                enemies.RemoveAt(i);
                kills++;
                if (kills >= 8) Finish(true);
                break;
            }
        }

        GameObject tracer = new GameObject("Gunshot Tracer");
        LineRenderer line = tracer.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.09f;
        line.endWidth = 0.015f;
        line.material = shotMaterial;
        Destroy(tracer, 0.12f);
    }

    private void SpawnEnemy()
    {
        bool bunny = Random.value > 0.5f;
        GameObject root = new GameObject(bunny ? "Zombunny" : "ZomBear");
        int edge = Random.Range(0, 4);
        float along = Random.Range(-9f, 9f);
        root.transform.position = edge == 0 ? new Vector3(-10f, 0f, along) :
            edge == 1 ? new Vector3(10f, 0f, along) :
            edge == 2 ? new Vector3(along, 0f, -10f) : new Vector3(along, 0f, 10f);
        CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
        capsule.center = Vector3.up;
        capsule.height = 2f;
        capsule.radius = 0.55f;
        AddModel(root.transform, bunny ? "Zombunny" : "ZomBear",
            bunny ? "ZomBunnyDiffuse" : "ZomBearDiffuse", 2f);
        enemies.Add(new Enemy
        {
            root = root, collider = capsule, speed = bunny ? 1.7f : 1.25f,
            nextAttack = Time.time + 2f, bunny = bunny
        });
    }

    private void BuildWorld()
    {
        gameCamera = new GameObject("Main Camera").AddComponent<Camera>();
        gameCamera.gameObject.AddComponent<AudioListener>();
        gameCamera.tag = "MainCamera";
        gameCamera.fieldOfView = 53f;
        gameCamera.transform.position = new Vector3(0f, 18f, -21f);
        gameCamera.transform.LookAt(new Vector3(0f, 0f, 1f));
        gameCamera.backgroundColor = new Color(0.12f, 0.1f, 0.18f);

        Light sunlight = new GameObject("Directional Light").AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.5f;
        sunlight.shadows = LightShadows.Soft;
        sunlight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientLight = new Color(0.47f, 0.49f, 0.54f);

        CreateCube("Arena Floor", new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 24f),
            new Color(0.23f, 0.27f, 0.34f));
        CreateCube("North Wall", new Vector3(0f, 0.7f, 12f), new Vector3(24f, 1.4f, 0.6f),
            new Color(0.37f, 0.25f, 0.39f));
        CreateCube("South Wall", new Vector3(0f, 0.7f, -12f), new Vector3(24f, 1.4f, 0.6f),
            new Color(0.37f, 0.25f, 0.39f));
        CreateCube("East Wall", new Vector3(12f, 0.7f, 0f), new Vector3(0.6f, 1.4f, 24f),
            new Color(0.37f, 0.25f, 0.39f));
        CreateCube("West Wall", new Vector3(-12f, 0.7f, 0f), new Vector3(0.6f, 1.4f, 24f),
            new Color(0.37f, 0.25f, 0.39f));

        AddProp("Train", "TrainDiffuse", new Vector3(-7f, 0f, 6.5f), 2.4f);
        AddProp("Robot", "RobotDiffuse", new Vector3(7f, 0f, 6.5f), 2.4f);

        player = new GameObject("Player - Survival Shooter Asset");
        playerController = player.AddComponent<CharacterController>();
        playerController.center = Vector3.up;
        playerController.height = 2f;
        playerController.radius = 0.5f;
        AddModel(player.transform, "Player", "PlayerDiffuse", 2f);

        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlit == null) unlit = Shader.Find("Unlit/Color");
        shotMaterial = new Material(unlit);
        shotMaterial.color = new Color(1f, 0.9f, 0.2f);
    }

    private static void AddProp(string model, string texture, Vector3 position, float height)
    {
        GameObject root = new GameObject(model + " - Survival Shooter Asset");
        root.transform.position = position;
        AddModel(root.transform, model, texture, height);
    }

    private static void AddModel(Transform parent, string modelName, string textureName, float height)
    {
        GameObject prefab = Resources.Load<GameObject>("SurvivalShooter/Models/" + modelName);
        Texture2D texture = Resources.Load<Texture2D>("SurvivalShooter/Textures/" + textureName);
        if (prefab == null || texture == null)
        {
            Debug.LogError("Missing Survival Shooter asset: " + modelName + " / " + textureName);
            return;
        }

        GameObject visual = Instantiate(prefab, parent);
        visual.name = modelName + " Model";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError("No renderer in Survival Shooter model: " + modelName);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y > 0.01f) visual.transform.localScale *= height / bounds.size.y;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.SetTexture("_BaseMap", texture);
        material.mainTexture = texture;
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++) materials[i] = material;
            renderer.sharedMaterials = materials;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        visual.transform.position += new Vector3(parent.position.x - bounds.center.x,
            parent.position.y - bounds.min.y, parent.position.z - bounds.center.z);
    }

    private static void CreateCube(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        cube.GetComponent<Renderer>().material = material;
    }

    private static AudioClip LoadSound(string name)
    {
        return Resources.Load<AudioClip>("SurvivalShooter/Audio/" + name);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null) soundEffects.PlayOneShot(clip, 0.5f);
    }

    private void Finish(bool victory)
    {
        finished = true;
        won = victory;
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
            hudStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(hudStyle) { fontSize = 44, alignment = TextAnchor.MiddleCenter };
        }
        GUI.Box(new Rect(15f, 15f, 430f, 140f), "");
        GUI.Label(new Rect(30f, 20f, 400f, 38f), "HEALTH  " + health, hudStyle);
        GUI.Label(new Rect(30f, 60f, 400f, 38f), "ZOMBIES  " + kills + " / 8", hudStyle);
        GUI.Label(new Rect(30f, 100f, 400f, 38f), "TIME  " + Mathf.CeilToInt(timeLeft) + "s", hudStyle);
        GUI.Label(new Rect(15f, Screen.height - 50f, 950f, 42f),
            "WASD: move  |  Mouse: aim  |  Click or Space: shoot", hudStyle);
        if (!finished) return;
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        GUI.Box(new Rect(centerX - 255f, centerY - 115f, 510f, 230f), "");
        GUI.Label(new Rect(centerX - 230f, centerY - 75f, 460f, 65f),
            won ? "YOU SURVIVED!" : "GAME OVER", titleStyle);
        GUI.Label(new Rect(centerX - 175f, centerY - 5f, 350f, 42f), "Press R to try again", hudStyle);
        if (GUI.Button(new Rect(centerX - 90f, centerY + 55f, 180f, 42f), "PLAY AGAIN")) Restart();
    }
}
