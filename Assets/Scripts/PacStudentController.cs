using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gridSize = 0.5f;
    public AudioClip moveClip;
    public AudioClip eatClip;
    private Vector2Int gridPos;
    private bool isMoving = false;
    private Vector2Int lastInput;
    private Vector2Int currentDir;
    private Vector2 prevWorldPos;
    private bool controllable = false;
    private AudioSource audioSource;
    private Animator anim;

    [Header("Particles")]
    public ParticleSystem dustFX;
    public GameObject pelletBurstPrefab;
    public GameObject deathBurstPrefab;

    [Header("Teleport")]
    [SerializeField] private Transform teleportLeft;
    [SerializeField] private Transform teleportRight;

    [Header("Respawn")]
    [SerializeField] private Vector2 respawnPoint = new Vector2(-5f, 3f);
    [SerializeField] private LayerMask wallMask;
    private Vector2Int targetGridPos;
    private Vector2 lastSafePos;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        Vector2 snapped = new Vector2(
            Mathf.Round(transform.position.x / gridSize) * gridSize,
            Mathf.Round(transform.position.y / gridSize) * gridSize
        );
        transform.position = snapped;
        gridPos = Vector2Int.RoundToInt(snapped / gridSize);
    }
    void Update()
    {
        if (!controllable)
        {
            if (anim) anim.SetBool("isMoving", false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.W)) currentDir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) currentDir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) currentDir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) currentDir = Vector2Int.right;
        if (!isMoving && currentDir != Vector2Int.zero)
        {
            if (CanMove(currentDir))
                StartCoroutine(MoveStep(gridPos + currentDir));
            else
                anim.SetBool("isMoving", false);
        }
    }
    IEnumerator MoveStep(Vector2Int newGridPos)
    {
        isMoving = true;
        Vector3 start = new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, 0f);
        Vector3 end = new Vector3(newGridPos.x * gridSize, newGridPos.y * gridSize, 0f);
        float t = 0f;
        anim.SetBool("isMoving", true);
        anim.SetFloat("dirX", currentDir.x);
        anim.SetFloat("dirY", currentDir.y);
        if (dustFX != null) dustFX.Play();
        if (audioSource && moveClip) audioSource.PlayOneShot(moveClip);
        lastSafePos = start;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        transform.position = end;
        gridPos = newGridPos;
        isMoving = false;
        if (dustFX != null) dustFX.Stop();
        if (CanMove(currentDir))
        {
            StartCoroutine(MoveStep(gridPos + currentDir));
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
        SnapToGrid();
    }
    void TryStep(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return;
        if (CanMove(dir))
        {
            currentDir = dir;
            StartCoroutine(MoveStep(gridPos + currentDir));
        }
    }
    bool CanMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;
        Vector2 targetPos = (Vector2)(gridPos + dir) * gridSize;
        Collider2D hit = Physics2D.OverlapBox(
            targetPos,
            new Vector2(gridSize * 0.8f, gridSize * 0.8f),
            0f,
            wallMask
        );
        return hit == null;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.I == null) return;
        Debug.Log("触发到：" + other.name);
        if (other.CompareTag("Pellet"))
        {
            GameManager.I.AddScore(10);
            if (pelletBurstPrefab) Instantiate(pelletBurstPrefab, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            if (audioSource && eatClip) audioSource.PlayOneShot(eatClip);
        }
        else if (other.CompareTag("PowerPellet"))
        {
            GameManager.I.TriggerPowerPellet();
            if (pelletBurstPrefab) Instantiate(pelletBurstPrefab, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Cherry"))
        {
            GameManager.I.AddScore(100);
            if (pelletBurstPrefab)
                Instantiate(pelletBurstPrefab, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            if (audioSource && eatClip)
                audioSource.PlayOneShot(eatClip);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Ghost"))
        {
            var g = other.GetComponent<GhostController>();
            if (g == null) return;
            if (g.CurrentState == GhostController.GhostState.Normal)
            {
                StartCoroutine(DoDie());
            }
            else if (g.CurrentState == GhostController.GhostState.Scared ||
                     g.CurrentState == GhostController.GhostState.Recovering)
            {
                g.Die();
                GameManager.I.OnGhostEaten(g);
            }
        }
        else if (other.name.Contains("TeleporterLeft"))
        {
            transform.position = new Vector3(8.6f, 1f, 0f);
        }
        else if (other.name.Contains("TeleporterRight"))
        {
            transform.position = new Vector3(-8.5f, 1f, 0f);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            transform.position = lastSafePos;
            gridPos = Vector2Int.RoundToInt(lastSafePos / gridSize);
            if (deathBurstPrefab)
                Instantiate(deathBurstPrefab, collision.contacts[0].point, Quaternion.identity);
        }
    }
    IEnumerator DoDie()
    {
        controllable = false;
        StopAllCoroutines();
        isMoving = false;
        anim.SetTrigger("Die");
        if (deathBurstPrefab) Instantiate(deathBurstPrefab, transform.position, Quaternion.identity);
        GameManager.I.LoseOneLifeAndRespawn();
        yield break;
    }
    void SnapToGrid()
    {
        Vector2 pos = new Vector2(
            Mathf.Round(transform.position.x / gridSize) * gridSize,
            Mathf.Round(transform.position.y / gridSize) * gridSize
        );
        transform.position = pos;
        gridPos = Vector2Int.RoundToInt(pos / gridSize);
        targetGridPos = gridPos;
    }
    void OnRoundReset()
    {
        StopAllCoroutines();
        anim.SetBool("isMoving", false);
        isMoving = false;
        Vector2 spawnPos = new Vector2(respawnPoint.x, respawnPoint.y);
        spawnPos = new Vector2(
            Mathf.Round(spawnPos.x / gridSize) * gridSize,
            Mathf.Round(spawnPos.y / gridSize) * gridSize
        );
        transform.position = spawnPos;
        gridPos = Vector2Int.RoundToInt(spawnPos / gridSize);
        targetGridPos = gridPos;
    }
    public void SetControllable(bool value)
    {
        controllable = value;
    }
    private void StopMoveAnim() { anim.SetBool("isMoving", false); }
    public void RespawnToStart()
    {
        OnRoundReset();
    }
}
