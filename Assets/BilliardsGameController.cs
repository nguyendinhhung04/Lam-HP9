using System.Collections;
using UnityEngine;

/// <summary>
/// Keep the mouse behind the cue ball to aim; hold and drag farther away to pull
/// the cue back, then release to strike. Pockets are visual only.
/// </summary>
[ExecuteAlways]
public class BilliardsGameController : MonoBehaviour
{
    private const float TableWidth = 14f, TableHeight = 7.4f, BallRadius = .28f;
    private const float CueLength = 3.1f, MaxPullback = 2.1f;
    private readonly Vector2 cueStart = new(-4.3f, 0f);
    private readonly Vector2 targetStart = new(2.2f, .7f);
    private Camera sceneCamera;
    private Rigidbody2D cueBall, targetBall;
    private SpriteRenderer cueStick;
    private Transform tableRoot;
    private Vector2 shotDirection = Vector2.right;
    private float pullback;
    private bool isPullingCue;
    private bool isStrikingCue;

    private void Awake() => InitializeTable();
    private void OnEnable() => InitializeTable();

    private void InitializeTable()
    {
        sceneCamera = Camera.main != null ? Camera.main : GetComponent<Camera>();
        if (sceneCamera != null)
        {
            sceneCamera.orthographic = true;
            sceneCamera.orthographicSize = 6.1f;
            sceneCamera.backgroundColor = new Color(.035f, .055f, .09f);
        }

        Physics2D.gravity = Vector2.zero;
        GameObject existingTable = GameObject.Find("Billiards Table");
        if (existingTable == null)
        {
            BuildTable();
            cueBall = CreateBall("Cue Ball", cueStart, Color.white);
            targetBall = CreateBall("Target Ball", targetStart, new Color(.95f, .34f, .12f));
            CreateCueStick();
        }
        else
        {
            tableRoot = existingTable.transform;
            cueBall = GameObject.Find("Cue Ball")?.GetComponent<Rigidbody2D>();
            targetBall = GameObject.Find("Target Ball")?.GetComponent<Rigidbody2D>();
            cueStick = GameObject.Find("Cue Stick")?.GetComponent<SpriteRenderer>();
            if (cueStick == null) CreateCueStick();
        }

        GameObject oldAimGuide = GameObject.Find("Aim Guide");
        if (oldAimGuide != null) oldAimGuide.SetActive(false);
        UpdateCueStickVisual();
    }

    private void Update()
    {
        if (cueBall == null || targetBall == null || cueStick == null) return;
        if (Input.GetKeyDown(KeyCode.R)) ResetTable();
        if (isStrikingCue) return;
        if (!BallsAreStill()) { cueStick.enabled = false; return; }

        UpdateDirectionFromMouse();
        if (Input.GetMouseButtonDown(0)) isPullingCue = true;
        if (isPullingCue && Input.GetMouseButton(0)) UpdatePullback();
        if (isPullingCue && Input.GetMouseButtonUp(0)) StrikeCueBall();
        UpdateCueStickVisual();
    }

    private void UpdateDirectionFromMouse()
    {
        if (sceneCamera == null) return;
        Vector2 mouseWorld = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = cueBall.position - mouseWorld;
        if (direction.sqrMagnitude > .01f) shotDirection = direction.normalized;
    }

    private void UpdatePullback()
    {
        Vector2 mouseWorld = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
        pullback = Mathf.Clamp(Vector2.Distance(mouseWorld, cueBall.position) - 1.15f, 0f, MaxPullback);
    }

    private void UpdateCueStickVisual()
    {
        if (cueStick == null || cueBall == null) return;
        cueStick.enabled = true;
        float distanceBehindBall = BallRadius + CueLength * .5f + .04f + pullback;
        cueStick.transform.position = cueBall.position - shotDirection * distanceBehindBall;
        cueStick.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg);
    }

    private void StrikeCueBall()
    {
        float force = Mathf.Lerp(2.5f, 16f, pullback / MaxPullback);
        isPullingCue = false;
        isStrikingCue = true;
        StartCoroutine(StrikeAnimation(force));
    }

    private IEnumerator StrikeAnimation(float force)
    {
        Vector3 pulledPosition = cueStick.transform.position;
        float contactDistance = BallRadius + CueLength * .5f + .04f;
        Vector3 contactPosition = cueBall.position - shotDirection * contactDistance;
        const float strikeDuration = .055f;
        float elapsed = 0f;

        while (elapsed < strikeDuration)
        {
            elapsed += Time.deltaTime;
            cueStick.transform.position = Vector3.Lerp(pulledPosition, contactPosition, elapsed / strikeDuration);
            yield return null;
        }

        cueBall.AddForce(shotDirection * force, ForceMode2D.Impulse);
        pullback = 0f;
        cueStick.enabled = false;
        isStrikingCue = false;
    }

    private bool BallsAreStill() => cueBall.linearVelocity.sqrMagnitude < .0025f && targetBall.linearVelocity.sqrMagnitude < .0025f;

    private void ResetTable()
    {
        ResetBall(cueBall, cueStart);
        ResetBall(targetBall, targetStart);
        pullback = 0f;
        isPullingCue = false;
        isStrikingCue = false;
    }

    private static void ResetBall(Rigidbody2D ball, Vector2 position)
    {
        ball.position = position;
        ball.linearVelocity = Vector2.zero;
        ball.angularVelocity = 0f;
    }

    private void OnGUI()
    {
        GUIStyle title = new(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
        GUIStyle hint = new(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 16, Screen.width, 32), "BIDA 2 BI", title);
        GUI.Label(new Rect(18, Screen.height - 82, 420, 24), "Đưa chuột ra sau bi cái để xoay gậy", hint);
        GUI.Label(new Rect(18, Screen.height - 55, 440, 24), "Giữ chuột và kéo lùi xa hơn để tăng lực", hint);
        GUI.Label(new Rect(18, Screen.height - 28, 350, 24), "Nhả chuột để đánh bi  •  R để làm lại", hint);
        GUI.Label(new Rect(Screen.width - 285, Screen.height - 36, 260, 24), "6 lỗ: chỉ hiển thị", hint);
        if (GUI.Button(new Rect(Screen.width - 150, 18, 120, 32), "LÀM LẠI")) ResetTable();
    }

    private void BuildTable()
    {
        tableRoot = new GameObject("Billiards Table").transform;
        CreateRectangle("Table Cloth", Vector2.zero, new Vector2(TableWidth, TableHeight), new Color(.035f, .38f, .27f), -2);
        CreateRectangle("Top Rail", new Vector2(0f, TableHeight / 2f + .17f), new Vector2(TableWidth + .55f, .48f), new Color(.24f, .10f, .035f), -1, true);
        CreateRectangle("Bottom Rail", new Vector2(0f, -TableHeight / 2f - .17f), new Vector2(TableWidth + .55f, .48f), new Color(.24f, .10f, .035f), -1, true);
        CreateRectangle("Left Rail", new Vector2(-TableWidth / 2f - .17f, 0f), new Vector2(.48f, TableHeight), new Color(.24f, .10f, .035f), -1, true);
        CreateRectangle("Right Rail", new Vector2(TableWidth / 2f + .17f, 0f), new Vector2(.48f, TableHeight), new Color(.24f, .10f, .035f), -1, true);
        Vector2[] pockets = { new(-6.85f, 3.55f), new(0f, 3.55f), new(6.85f, 3.55f), new(-6.85f, -3.55f), new(0f, -3.55f), new(6.85f, -3.55f) };
        foreach (Vector2 pocket in pockets) CreateCircle("Pocket (visual only)", pocket, .42f, new Color(.015f, .015f, .02f), 0, false);
    }

    private Rigidbody2D CreateBall(string objectName, Vector2 position, Color color)
    {
        GameObject ball = CreateCircle(objectName, position, BallRadius, color, 3, true);
        Rigidbody2D body = ball.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.linearDamping = 1.25f;
        body.angularDamping = 1.5f;
        body.sharedMaterial = CreateBouncyMaterial();
        return body;
    }

    private void CreateCueStick()
    {
        cueStick = CreateRectangle("Cue Stick", Vector2.zero, new Vector2(CueLength, .18f), new Color(.57f, .28f, .07f), 4).GetComponent<SpriteRenderer>();
    }

    private GameObject CreateRectangle(string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder, bool addCollider = false)
    {
        GameObject rectangle = new(objectName);
        SpriteRenderer renderer = rectangle.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSprite(false);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        rectangle.transform.position = position;
        rectangle.transform.localScale = size;
        rectangle.transform.SetParent(tableRoot, true);
        if (addCollider)
        {
            BoxCollider2D collider = rectangle.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.sharedMaterial = CreateBouncyMaterial();
        }
        return rectangle;
    }

    private GameObject CreateCircle(string objectName, Vector2 position, float radius, Color color, int sortingOrder, bool addCollider)
    {
        GameObject circle = new(objectName);
        SpriteRenderer renderer = circle.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSprite(true);
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        circle.transform.position = position;
        circle.transform.localScale = Vector3.one * radius * 2f;
        circle.transform.SetParent(tableRoot, true);
        if (addCollider) circle.AddComponent<CircleCollider2D>().radius = .5f;
        return circle;
    }

    private static Sprite CreateSprite(bool circle)
    {
        Texture2D texture = new(64, 64, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
            pixels[y * 64 + x] = !circle || distance <= 31.5f ? Color.white : Color.clear;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64f);
    }

    private static PhysicsMaterial2D CreateBouncyMaterial() => new() { bounciness = .9f, friction = .12f };
}
