using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gridSize = 0.5f;
    public AudioClip moveClip;
    public AudioClip eatClip;
    public ParticleSystem dustFX;
    private Vector2Int gridPos;
    private Vector2Int targetGridPos;
    private bool isMoving = false;
    private Vector2Int lastInput;
    private Vector2Int currentInput;
    private AudioSource audioSource;
    private Animator anim;
    [Header("Particles")]
    public GameObject pelletBurstPrefab;
    public GameObject deathBurstPrefab;
    void Start()
    {
        gridPos = Vector2Int.RoundToInt(transform.position / gridSize);
        targetGridPos = gridPos;
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        Vector2Int input = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) input = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S)) input = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) input = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D)) input = Vector2Int.right;
        if (input != Vector2Int.zero)
            lastInput = input;
        if (!isMoving)
        {
            TryMove();
        }
    }
    void TryMove()
    {
        if (CanMove(lastInput))
        {
            currentInput = lastInput;
            StartCoroutine(LerpTo(gridPos + currentInput));
        }
        else if (CanMove(currentInput))
        {
            StartCoroutine(LerpTo(gridPos + currentInput));
        }
    }
    IEnumerator LerpTo(Vector2Int newGridPos)
    {
        isMoving = true;
        Vector3 start = new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, 0f);
        Vector3 end = new Vector3(newGridPos.x * gridSize, newGridPos.y * gridSize, 0f);
        float t = 0f;
        anim.SetBool("isMoving", true);
        Vector2Int dir = newGridPos - gridPos;
        anim.SetFloat("dirX", dir.x);
        anim.SetFloat("dirY", dir.y);
        if (dustFX != null) dustFX.Play();
        if (dustFX != null) dustFX.Stop();
        if (audioSource && moveClip) audioSource.PlayOneShot(moveClip);
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        transform.position = end;
        gridPos = newGridPos;
        isMoving = false;
        anim.SetBool("isMoving", false);
        if (dustFX != null) dustFX.Stop();
        TryMove();
    }
    bool CanMove(Vector2Int dir)
    {
        Vector3 origin = new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, -1f);
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, gridSize);
        if (hit.collider == null) return true;
        return !hit.collider.CompareTag("Wall");
    }
    void SpawnPelletFX(Vector3 at)
    {
        if (pelletBurstPrefab) Instantiate(pelletBurstPrefab, at, Quaternion.identity);
        AudioManager.I.PlaySFX(AudioManager.I.pelletClip, 0.7f);
    }
    void SpawnDeathFX(Vector3 at)
    {
        if (deathBurstPrefab) Instantiate(deathBurstPrefab, at, Quaternion.identity);
        AudioManager.I.PlaySFX(AudioManager.I.deathClip, 1f);
    }
}
