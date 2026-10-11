using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbCollectorGame : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusic;

    private readonly Vector3[] pickupPositions =
    {
        new Vector3(-6f, 0.8f, -3.5f), new Vector3(-2f, 0.8f, -3.5f),
        new Vector3(2f, 0.8f, -3.5f), new Vector3(6f, 0.8f, -3.5f),
        new Vector3(-6f, 0.8f, 3.5f), new Vector3(-2f, 0.8f, 3.5f),
        new Vector3(2f, 0.8f, 3.5f), new Vector3(6f, 0.8f, 3.5f)
    };

    private Rigidbody playerBody;
    private PickupItem[] pickups;
    private int collected;
    private float timeLeft = 75f;
    private bool finished;
    private bool won;
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;

    private void Start()
    {
        BuildWorld();
        if (backgroundMusic != null)
        {
            AudioSource music = gameObject.AddComponent<AudioSource>();
            music.clip = backgroundMusic;
            music.loop = true;
            music.volume = 0.3f;
            music.Play();
        }
    }

    private void Update()
    {
        if (finished)
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            return;
        }

        for (int i = 0; i < pickups.Length; i++)
        {
            PickupItem item = pickups[i];
            if (!item.gameObject.activeSelf) continue;
            Vector3 offset = playerBody.position - item.transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= 1.05f * 1.05f) Collect(item);
        }
        if (finished) return;

        timeLeft = Mathf.Max(0f, timeLeft - Time.deltaTime);
        if (timeLeft <= 0f) Finish(false);
    }

    private void FixedUpdate()
    {
        if (finished || playerBody == null) return;
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);
        playerBody.linearVelocity = new Vector3(input.x * 8f, 0f, input.z * 8f);
    }

    public void Collect(PickupItem item)
    {
        if (finished || item == null || !item.gameObject.activeSelf) return;
        item.gameObject.SetActive(false);
        collected++;
        if (collected >= pickupPositions.Length) Finish(true);
    }

    private void Finish(bool victory)
    {
        finished = true;
        won = victory;
        if (playerBody != null) playerBody.linearVelocity = Vector3.zero;
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BuildWorld()
    {
        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.gameObject.AddComponent<AudioListener>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 10.5f;
        camera.transform.position = new Vector3(0f, 16f, -12f);
        camera.transform.LookAt(Vector3.zero);
        camera.backgroundColor = new Color(0.08f, 0.11f, 0.18f);

        Light sunlight = new GameObject("Directional Light").AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.3f;
        sunlight.transform.rotation = Quaternion.Euler(45f, -25f, 0f);
        RenderSettings.ambientLight = new Color(0.45f, 0.5f, 0.6f);

        CreateCube("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(19f, 1f, 13f), new Color(0.15f, 0.27f, 0.34f));
        CreateCube("North Wall", new Vector3(0f, 0.6f, 6.5f), new Vector3(19f, 1.2f, 0.7f), new Color(0.1f, 0.17f, 0.25f));
        CreateCube("South Wall", new Vector3(0f, 0.6f, -6.5f), new Vector3(19f, 1.2f, 0.7f), new Color(0.1f, 0.17f, 0.25f));
        CreateCube("East Wall", new Vector3(9.5f, 0.6f, 0f), new Vector3(0.7f, 1.2f, 13f), new Color(0.1f, 0.17f, 0.25f));
        CreateCube("West Wall", new Vector3(-9.5f, 0.6f, 0f), new Vector3(0.7f, 1.2f, 13f), new Color(0.1f, 0.17f, 0.25f));

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player Ball";
        player.transform.position = new Vector3(0f, 0.65f, 0f);
        player.transform.localScale = Vector3.one * 1.3f;
        player.GetComponent<Renderer>().material = MakeMaterial(new Color(0.18f, 0.8f, 0.96f));
        playerBody = player.AddComponent<Rigidbody>();
        playerBody.mass = 1f;
        playerBody.useGravity = false;
        playerBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        player.AddComponent<PlayerPickup>().game = this;

        pickups = new PickupItem[pickupPositions.Length];
        for (int i = 0; i < pickupPositions.Length; i++)
        {
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickup.name = "Star " + (i + 1);
            pickup.transform.position = pickupPositions[i];
            pickup.transform.localScale = Vector3.one * 0.75f;
            pickup.GetComponent<Renderer>().material = MakeMaterial(new Color(1f, 0.73f, 0.15f));
            pickup.GetComponent<SphereCollider>().isTrigger = true;
            pickups[i] = pickup.AddComponent<PickupItem>();
        }
    }

    private static void CreateCube(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().material = MakeMaterial(color);
    }

    private static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            hudStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(hudStyle) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
        }

        GUI.Box(new Rect(15f, 15f, 390f, 95f), "");
        GUI.Label(new Rect(30f, 22f, 360f, 35f), "ORBS  " + collected + " / " + pickupPositions.Length, hudStyle);
        GUI.Label(new Rect(30f, 60f, 360f, 35f), "TIME  " + Mathf.CeilToInt(timeLeft) + "s", hudStyle);
        GUI.Label(new Rect(15f, Screen.height - 48f, 500f, 38f), "Move: WASD / Arrow keys", hudStyle);

        if (!finished) return;
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        GUI.Box(new Rect(centerX - 220f, centerY - 105f, 440f, 210f), "");
        GUI.Label(new Rect(centerX - 200f, centerY - 75f, 400f, 60f), won ? "YOU WIN!" : "TIME UP!", titleStyle);
        GUI.Label(new Rect(centerX - 160f, centerY - 5f, 320f, 40f), "Press R to play again", hudStyle);
        if (GUI.Button(new Rect(centerX - 90f, centerY + 50f, 180f, 40f), "PLAY AGAIN")) Restart();
    }
}
