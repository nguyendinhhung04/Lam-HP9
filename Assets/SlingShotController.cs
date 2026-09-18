using UnityEngine;

/// <summary>
/// Kéo quả bóng ngược với hướng muốn bắn, rồi thả chuột trái để phóng nó đi.
/// Nhấn R để đưa bóng về vị trí ban đầu và bắn lại.
/// </summary>
public class SlingShotController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D bird;

    [Header("Sling settings")]
    [SerializeField, Min(0.1f)] private float maxDragDistance = 2f;
    [SerializeField, Min(0.1f)] private float launchMultiplier = 8f;
    [SerializeField] private float launchedGravityScale = 1f;

    private Camera mainCamera;
    private Collider2D birdCollider;
    private LineRenderer aimLine;
    private Vector2 startPosition;
    private bool isDragging;
    private bool hasLaunched;

    private void Awake()
    {
        mainCamera = Camera.main;
        birdCollider = bird.GetComponent<Collider2D>();
        startPosition = bird.position;
        bird.bodyType = RigidbodyType2D.Kinematic;
        bird.gravityScale = 0f;
        CreateAimLine();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetBird();
        if (hasLaunched || mainCamera == null) return;

        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && birdCollider.OverlapPoint(mouseWorld))
        {
            isDragging = true;
            aimLine.enabled = true;
        }

        if (!isDragging) return;

        Vector2 dragOffset = Vector2.ClampMagnitude(mouseWorld - startPosition, maxDragDistance);
        bird.position = startPosition + dragOffset;
        aimLine.SetPosition(0, startPosition);
        aimLine.SetPosition(1, bird.position);

        if (Input.GetMouseButtonUp(0)) Launch();
    }

    private void Launch()
    {
        isDragging = false;
        hasLaunched = true;
        aimLine.enabled = false;
        Vector2 launchDirection = startPosition - bird.position;
        bird.bodyType = RigidbodyType2D.Dynamic;
        bird.gravityScale = launchedGravityScale;
        bird.AddForce(launchDirection * launchMultiplier, ForceMode2D.Impulse);
    }

    private void ResetBird()
    {
        isDragging = false;
        hasLaunched = false;
        bird.bodyType = RigidbodyType2D.Kinematic;
        bird.position = startPosition;
        bird.linearVelocity = Vector2.zero;
        bird.angularVelocity = 0f;
        bird.gravityScale = 0f;
        aimLine.enabled = false;
    }

    private void CreateAimLine()
    {
        GameObject lineObject = new("Aim Line");
        aimLine = lineObject.AddComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.startWidth = 0.06f;
        aimLine.endWidth = 0.06f;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = new Color(0.35f, 0.18f, 0.08f, 1f);
        aimLine.endColor = aimLine.startColor;
        aimLine.sortingOrder = 10;
        aimLine.useWorldSpace = true;
        aimLine.enabled = false;
    }
}
