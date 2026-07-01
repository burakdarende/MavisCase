using UnityEngine;
using UnityEngine.InputSystem;

public class GameManagerSimple : MonoBehaviour
{
    [Header("Player Efekti")]
    [SerializeField] private GameObject playerEffectPrefab;

    [Header("Effect - Target Üzerinde")]
    [SerializeField] private GameObject effectPrefab;

    [Header("Transforms")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform enemyTransform;

    [Header("Spawn Offset")]
    [SerializeField] private float spawnHeightOffset;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SpawnEffectOnPlayer();
            SpawnEffectOnTarget();
        }
    }

    void SpawnEffectOnPlayer()
    {
        if (playerEffectPrefab == null || playerTransform == null)
            return;

        GameObject spawnedEffect = Instantiate(
            playerEffectPrefab,
            GetSpawnPosition(playerTransform),
            Quaternion.identity);
        spawnedEffect.transform.SetParent(null);

        foreach (ParticleSystem ps in spawnedEffect.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play(true);
    }

    void SpawnEffectOnTarget()
    {
        if (effectPrefab == null || enemyTransform == null)
            return;

        GameObject spawnedEffect = Instantiate(
            effectPrefab,
            GetSpawnPosition(enemyTransform),
            Quaternion.identity);
        spawnedEffect.transform.SetParent(null);

        foreach (ParticleSystem ps in spawnedEffect.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play(true);
    }

    Vector3 GetSpawnPosition(Transform character)
    {
        Vector3 position;

        if (character == null)
            position = Vector3.zero;
        else
        {
            Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                position = character.position;
            else
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                position = bounds.center;
            }
        }

        return position + Vector3.up * spawnHeightOffset;
    }
}
