using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GhostController : MonoBehaviour
{
    public enum GhostState { Normal = 0, Scared = 1, Recovering = 2, Dead = 3 }
    [Header("Spawn & Home")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform homePoint;
    [SerializeField] private float deadReturnSpeed = 3f;
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float gridSize = 0.5f;
    [SerializeField] private LayerMask wallMask;
    private Animator anim;
    private bool movable = false;
    private bool isMoving = false;
    private Coroutine moveRoutine;
    private Coroutine deadRoutine;
    private Vector2Int gridPos;
    private Vector2Int currentDir = Vector2Int.left;
    private bool IsMobileState => CurrentState != GhostState.Dead;
    private void HardStop()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        isMoving = false;
    }
    private void SnapToGridAt(Vector3 p)
    {
        Vector2 snapped = new Vector2(
            Mathf.Round(p.x / gridSize) * gridSize,
            Mathf.Round(p.y / gridSize) * gridSize
        );
        transform.position = snapped;
        gridPos = Vector2Int.RoundToInt(snapped / gridSize);
    }
    private void Awake()
    {
        anim = GetComponent<Animator>();
        SetAnim();
    }
    public GhostState CurrentState { get; private set; } = GhostState.Normal;
    private void EnsureMoving()
    {
        if (movable && IsMobileState && moveRoutine == null)
            TryStep(currentDir);
    }
    private void Start()
    {
        Vector2 snapped = new Vector2(
            Mathf.Round(transform.position.x / gridSize) * gridSize,
            Mathf.Round(transform.position.y / gridSize) * gridSize
        );
        transform.position = snapped;
        gridPos = Vector2Int.RoundToInt(snapped / gridSize);
        if (name.Contains("Red")) currentDir = Vector2Int.right;
        else if (name.Contains("Blue")) currentDir = Vector2Int.left;
        else if (name.Contains("Pink")) currentDir = Vector2Int.up;
        else if (name.Contains("Orange")) currentDir = Vector2Int.down;
        else currentDir = Vector2Int.left;
        StartCoroutine(DelayedStart());
    }
    private void Update()
    {
        if (!movable) return;
        if (CurrentState == GhostState.Scared || CurrentState == GhostState.Recovering)
        {
            if (moveRoutine == null)
            {
                TryStep(currentDir);
            }
            return;
        }
        if (!IsMobileState) return;
        if (CurrentState == GhostState.Dead) return;
        if (moveRoutine == null)
        {
            TryStep(currentDir);
        }
    }
    private void TryStep(Vector2Int dir)
    {
        if (!CanMove(dir))
        {
            List<Vector2Int> dirs = new List<Vector2Int>
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
            };
            dirs.Remove(-dir);
            foreach (var d in dirs)
            {
                if (CanMove(d))
                {
                    dir = d;
                    break;
                }
            }
        }
        currentDir = dir;
        Vector2Int nextPos = gridPos + dir;
        moveRoutine = StartCoroutine(MoveStep(nextPos));
    }
    private IEnumerator MoveStep(Vector2Int newGridPos)
    {
        if (isMoving) yield break;
        isMoving = true;
        Vector3 start = new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, 0f);
        Vector3 end = new Vector3(newGridPos.x * gridSize, newGridPos.y * gridSize, 0f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        transform.position = end;
        gridPos = newGridPos;
        isMoving = false;
        yield return new WaitForSeconds(0.01f);
        if (movable && IsMobileState)
        {
            if (CanMove(currentDir))
            {
                StartCoroutine(MoveStep(gridPos + currentDir));
            }
            else
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                for (int i = 0; i < 4; i++)
                {
                    Vector2Int tryDir = dirs[Random.Range(0, dirs.Length)];
                    if (CanMove(tryDir))
                    {
                        currentDir = tryDir;
                        StartCoroutine(MoveStep(gridPos + currentDir));
                        break;
                    }
                }
            }
        }
    }
    private bool CanMove(Vector2Int dir)
    {
        Vector2 targetPos = (Vector2)(gridPos + dir) * gridSize;
        Collider2D hit = Physics2D.OverlapBox(
            targetPos,
            new Vector2(gridSize * 0.8f, gridSize * 0.8f),
            0f,
            wallMask
        );
        return hit == null;
    }
    public void SetMovable(bool canMove)
    {
        movable = canMove;
        if (canMove && moveRoutine == null && CurrentState != GhostState.Dead)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            currentDir = dirs[Random.Range(0, dirs.Length)];
            TryStep(currentDir);
        }
    }
    public void ResetToSpawn_Normal()
    {
        if (deadRoutine != null) StopCoroutine(deadRoutine);
        HardStop();
        CurrentState = GhostState.Normal;
        SetAnim();
        if (spawnPoint != null) SnapToGridAt(spawnPoint.position);
        else SnapToGridAt(transform.position);
        if (movable && CurrentState != GhostState.Dead)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            currentDir = dirs[Random.Range(0, dirs.Length)];
            moveRoutine = StartCoroutine(MoveStep(gridPos + currentDir));
        }
    }
    public void SetScared()
    {
        if (CurrentState == GhostState.Dead) return;
        CurrentState = GhostState.Scared;
        SetAnim();
        if (movable && moveRoutine == null)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            currentDir = dirs[Random.Range(0, dirs.Length)];
            TryStep(currentDir);
        }
        EnsureMoving();
    }
    public void SetRecovering()
    {
        if (CurrentState == GhostState.Dead) return;
        CurrentState = GhostState.Recovering;
        SetAnim();
        if (movable && moveRoutine == null)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            currentDir = dirs[Random.Range(0, dirs.Length)];
            TryStep(currentDir);
        }
        EnsureMoving();
    }
    public void SetNormal()
    {
        CurrentState = GhostState.Normal;
        SetAnim();
        EnsureMoving();
    }
    public void SetNormalIfNotDead()
    {
        if (CurrentState != GhostState.Dead) SetNormal();
    }
    public void Die()
    {
        if (moveRoutine != null) { StopCoroutine(moveRoutine); moveRoutine = null; }
        if (deadRoutine != null) StopCoroutine(deadRoutine);
        deadRoutine = StartCoroutine(DeadThenRevive());
    }
    private IEnumerator DeadThenRevive()
    {
        CurrentState = GhostState.Dead;
        SetAnim();
        while (Vector3.Distance(transform.position, homePoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                homePoint.position,
                deadReturnSpeed * Time.deltaTime
            );
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        if (spawnPoint) transform.position = spawnPoint.position;
        if (spawnPoint != null)
            transform.position = spawnPoint.position;
        Vector2 snapped = new Vector2(
            Mathf.Round(transform.position.x / gridSize) * gridSize,
            Mathf.Round(transform.position.y / gridSize) * gridSize
        );
        transform.position = snapped;
        gridPos = Vector2Int.RoundToInt(snapped / gridSize);
        CurrentState = GhostState.Normal;
        SetAnim();
        yield return new WaitForSeconds(Random.Range(0.3f, 1.0f));
        if (movable)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            currentDir = dirs[Random.Range(0, dirs.Length)];
            StartCoroutine(MoveStep(gridPos + currentDir));
        }
        deadRoutine = null;
        EnsureMoving();
    }
    private void SetAnim()
    {
        if (anim != null)
            anim.SetInteger("State", (int)CurrentState);
    }
    private IEnumerator WanderRoutine()
    {
        while (movable && CurrentState == GhostState.Normal)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3f)); // 每1.5~3秒换一次方向
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            Vector2Int newDir = dirs[Random.Range(0, dirs.Length)];
            if (CanMove(newDir))
            {
                currentDir = newDir;
                TryStep(currentDir);
            }
        }
    }
    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));
        if (movable && CurrentState == GhostState.Normal)
            StartCoroutine(MoveStep(gridPos + currentDir));
    }
}

