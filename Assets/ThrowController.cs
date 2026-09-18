using UnityEngine;

public class ThrowController : MonoBehaviour
{
    [Header("References")]
    public Transform arrow;        // Kéo object mũi tên vào đây
    public Rigidbody2D ball;       // Kéo Rigidbody2D của quả bóng vào đây

    [Header("Angle Settings")]
    public float minAngle = 0f;    // Góc nhỏ nhất
    public float maxAngle = 90f;   // Góc lớn nhất
    public float rotateSpeed = 60f; // Tốc độ thay đổi góc (độ / giây)

    [Header("Throw Settings")]
    public float throwForce = 10f; // Lực ném
    public bool resetBallOnThrow = true; // Có reset vị trí bóng trước khi ném không
    public Vector2 ballStartPosition;    // Vị trí ban đầu của bóng (nếu reset)

    [Header("Arrow Orbit Settings")]
    public float arrowDistance = 1f; // Khoảng cách từ tâm bóng đến mũi tên

    private float currentAngle;
    private bool increasing = true;

    [SerializeField]
    private bool isThrowed = false;

    void Start()
    {
        ball.gravityScale = 0f;
        currentAngle = minAngle;
        if (ball != null)
            ballStartPosition = ball.transform.position;
    }

    void Update()
    {
        if (!isThrowed)
        {
            UpdateAngle();
            UpdateArrowRotation();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isThrowed)
        {
            ThrowBall();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetThrow();
        }
    }

    void UpdateAngle()
    {
        // Cho góc chạy qua lại giữa minAngle và maxAngle (giống thanh sức mạnh trong game bắn bi)
        if (increasing)
        {
            currentAngle += rotateSpeed * Time.deltaTime;
            if (currentAngle >= maxAngle)
            {
                currentAngle = maxAngle;
                increasing = false;
            }
        }
        else
        {
            currentAngle -= rotateSpeed * Time.deltaTime;
            if (currentAngle <= minAngle)
            {
                currentAngle = minAngle;
                increasing = true;
            }
        }
    }

    void UpdateArrowRotation()
    {
        if (arrow != null && ball != null)
        {
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Đặt mũi tên ở vị trí xoay quanh quả bóng, cách tâm bóng một khoảng arrowDistance
            Vector2 ballPos = ball.transform.position;
            arrow.position = ballPos + direction * arrowDistance;

            // Xoay mũi tên để nó luôn hướng ra ngoài (theo đúng hướng ném)
            arrow.rotation = Quaternion.Euler(0f, 0f, currentAngle - 90f);
        }
    }

    void ThrowBall()
    {
        if (ball == null) return;

        isThrowed = true;

        // Tính hướng ném dựa theo currentAngle (0 = ngang phải, 90 = thẳng lên)
        ball.gravityScale = 1f;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        ball.AddForce(direction * throwForce, ForceMode2D.Impulse);
    }

    void ResetThrow()
    {
        isThrowed = false;

        if (ball != null)
        {
            ball.gravityScale = 0f;
            ball.transform.position = ballStartPosition;
            ball.linearVelocity = Vector2.zero; // Unity 6+ dùng linearVelocity; nếu bản cũ hơn dùng ball.velocity
            ball.angularVelocity = 0f;
        }

        // Đưa góc về lại điểm bắt đầu để mũi tên chạy lại từ đầu
        currentAngle = minAngle;
        increasing = true;
    }
}