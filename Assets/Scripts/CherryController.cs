using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject cherryPrefab;
    public float respawnDelay = 5f;
    public Vector2 levelBounds = new Vector2(8, 5);
    public Vector3 levelCenter = Vector3.zero;

    [Header("Move Settings")]
    public float speed = 2f;
    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }
    void Update()
    {

    }
    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        while (true)
        {
            Vector3 start = GetRandomEdgePosition();
            Vector3 end = -start;
            if (AudioManager.I != null)
                AudioManager.I.PlaySFX(AudioManager.I.cherrySpawnClip, 1f);
            GameObject cherry = Instantiate(cherryPrefab, start, Quaternion.identity);
            StartCoroutine(MoveCherry(cherry, start, end));
            yield return new WaitForSeconds(respawnDelay);
        }
    }
    private IEnumerator MoveCherry(GameObject cherry, Vector3 start, Vector3 end)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed / Vector3.Distance(start, end);
            if (cherry != null)
                cherry.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        if (cherry != null) Destroy(cherry);
    }
    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: return new Vector3(-levelBounds.x, Random.Range(-levelBounds.y, levelBounds.y), 0); // 左
            case 1: return new Vector3(levelBounds.x, Random.Range(-levelBounds.y, levelBounds.y), 0);  // 右
            case 2: return new Vector3(Random.Range(-levelBounds.x, levelBounds.x), levelBounds.y, 0);  // 上
            default: return new Vector3(Random.Range(-levelBounds.x, levelBounds.x), -levelBounds.y, 0);// 下
        }
    }
}