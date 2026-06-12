using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

public class Player : MonoBehaviour
{
    public float moveSpeed = 8f;
    public Transform firePoint;
    public List<KeyCode> shootingKeys = new List<KeyCode> { KeyCode.Space };

    private GameObject bulletPrefab;
    private float lastShotTime;
    private float fireRate = 0.25f;

    private void Start()
    {
        bulletPrefab = Resources.Load<GameObject>("bullet/bullet");
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Player could not find Resources/bullet/bullet.");
            return;
        }

        Bullet bullet = bulletPrefab.GetComponent<Bullet>();
        if (bullet != null)
        {
            fireRate = Mathf.Max(0.01f, bullet.fireRate);
        }
    }

    private void Update()
    {
        Move();
        Shoot();
    }

    private void Move()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector2 moveInput = ReadMoveInput();
        float moveX = moveInput.x;
        float moveY = moveInput.y;
        Vector3 direction = new Vector3(moveX, moveY, 0f).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        viewportPosition.x = Mathf.Clamp01(viewportPosition.x);
        viewportPosition.y = Mathf.Clamp01(viewportPosition.y);
        viewportPosition.z = Mathf.Max(0f, viewportPosition.z);

        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(viewportPosition);
        worldPosition.z = 0f;
        transform.position = worldPosition;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || Time.time < lastShotTime + fireRate)
        {
            return;
        }

        for (int i = 0; i < shootingKeys.Count; i++)
        {
            if (!IsShootPressed(shootingKeys[i]))
            {
                continue;
            }

            Vector3 spawnPosition = firePoint != null
                ? firePoint.position
                : transform.position + Vector3.forward * 1.1f;

            Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            lastShotTime = Time.time;
            break;
        }
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 moveInput = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                moveInput.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                moveInput.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                moveInput.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                moveInput.y += 1f;
            }
        }
#endif

        if (moveInput == Vector2.zero)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
        }

        return Vector2.ClampMagnitude(moveInput, 1f);
    }

    private bool IsShootPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && TryGetKeyControl(keyboard, key, out KeyControl keyControl) && keyControl.wasPressedThisFrame)
        {
            return true;
        }
#endif

        return Input.GetKeyDown(key);
    }

#if ENABLE_INPUT_SYSTEM
    private bool TryGetKeyControl(Keyboard keyboard, KeyCode key, out KeyControl keyControl)
    {
        switch (key)
        {
            case KeyCode.Space:
                keyControl = keyboard.spaceKey;
                return true;
            case KeyCode.Return:
                keyControl = keyboard.enterKey;
                return true;
            case KeyCode.KeypadEnter:
                keyControl = keyboard.numpadEnterKey;
                return true;
            case KeyCode.UpArrow:
                keyControl = keyboard.upArrowKey;
                return true;
            case KeyCode.DownArrow:
                keyControl = keyboard.downArrowKey;
                return true;
            case KeyCode.LeftArrow:
                keyControl = keyboard.leftArrowKey;
                return true;
            case KeyCode.RightArrow:
                keyControl = keyboard.rightArrowKey;
                return true;
            case KeyCode.W:
                keyControl = keyboard.wKey;
                return true;
            case KeyCode.A:
                keyControl = keyboard.aKey;
                return true;
            case KeyCode.S:
                keyControl = keyboard.sKey;
                return true;
            case KeyCode.D:
                keyControl = keyboard.dKey;
                return true;
            default:
                keyControl = null;
                return false;
        }
    }
#endif

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("enemy"))
        {
            return;
        }

        if (GameLogic.Instance != null)
        {
            GameLogic.Instance.RegisterPlayerHit();
        }

        if (GameLogic.Instance != null && GameLogic.Instance.currentHits >= GameLogic.Instance.maxHits)
        {
            Destroy(gameObject);
        }
    }
}
