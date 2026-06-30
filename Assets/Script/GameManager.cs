using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Player Efekti")]
    [SerializeField] private GameObject playerEffectPrefab;

    [Header("Effect - Target Üzerinde")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private float targetEffectDelay = 0.5f;

    [Header("Frost Path - Player ile Target Arası")]
    [SerializeField] private GameObject frostPathPrefab;
    [SerializeField] private int pathPieceCount = 8;
    [SerializeField] private float pathDelay = 0.02f;
    [SerializeField] private float pathSideOffset = 0.3f;

    [Header("Transforms")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform enemyTransform;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(SpawnSequence());
        }
    }

    IEnumerator SpawnSequence()
    {
        // önce player efekti
        SpawnEffectOnPlayer();

        // path ile aynı anda başlar
        StartCoroutine(SpawnFrostPath());

        // delay sonra target efekti
        yield return new WaitForSeconds(targetEffectDelay);
        SpawnEffectOnTarget();
    }

 void SpawnEffectOnPlayer()
{
    if (playerEffectPrefab == null || playerTransform == null) return;

    // playerın pozisyonunun biraz üstüne spawn et
    Vector3 spawnPos = playerTransform.position + Vector3.up * 1f;

    GameObject spawnedEffect = Instantiate(playerEffectPrefab, spawnPos, Quaternion.identity);
    spawnedEffect.transform.SetParent(null);

    foreach (ParticleSystem ps in spawnedEffect.GetComponentsInChildren<ParticleSystem>(true))
        ps.Play(true);
}

    void SpawnEffectOnTarget()
    {
        if (effectPrefab == null || enemyTransform == null) return;

        GameObject spawnedEffect = Instantiate(effectPrefab, enemyTransform.position, Quaternion.identity);
        spawnedEffect.transform.SetParent(null);

        foreach (ParticleSystem ps in spawnedEffect.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play(true);
    }

    IEnumerator SpawnFrostPath()
    {
        if (frostPathPrefab == null || playerTransform == null || enemyTransform == null) yield break;

        Vector3 start = playerTransform.position;
        Vector3 end = enemyTransform.position;
        Vector3 direction = new Vector3(end.x - start.x, 0f, end.z - start.z).normalized;

        if (direction == Vector3.zero) yield break;

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

        for (int i = 0; i < pathPieceCount; i++)
        {
            float t = Mathf.Lerp(0.1f, 0.9f, (float)i / (pathPieceCount - 1));
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos += perpendicular * Random.Range(-pathSideOffset, pathSideOffset);
            pos.y = start.y;

            GameObject piece = Instantiate(frostPathPrefab, pos, rotation);
            piece.transform.SetParent(null);

            foreach (ParticleSystem ps in piece.GetComponentsInChildren<ParticleSystem>(true))
                ps.Play(true);

            yield return new WaitForSeconds(pathDelay);
        }
    }
}