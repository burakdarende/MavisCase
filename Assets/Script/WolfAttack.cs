using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WolfAttack : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject wolfPrefab;
    [SerializeField] private Vector3 wolfScale = Vector3.one;

    [Header("Launch")]
    [SerializeField] private Transform launchFrom;
    [SerializeField] private Transform launchTarget;
    [SerializeField] private SkinnedMeshRenderer launchErodeTarget;

    [Header("Effects")]
    [SerializeField] private GameObject fromEffect;
    [SerializeField] private GameObject impactEffect;

    [Header("Movement")]
    [SerializeField] private Animator wolfAnimator;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float arrivalDistance = 0.3f;
    [SerializeField] private string runAnimation = "wolf_rig|running";

    [Header("Wolf Dissolve")]
    [SerializeField] private SkinnedMeshRenderer wolfMesh;
    [SerializeField] private float wolfDisappearDuration = 1f;

    [Header("Target Erosion")]
    [SerializeField] private float erodeRate = 0.03f;
    [SerializeField] private float erodeRefreshRate = 0.01f;
    [SerializeField] private float erodeDelay = 1.25f;

    SkinnedMeshRenderer activeErodeTarget;
    InputAction spaceKey;

    void Awake()
    {
        spaceKey = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        spaceKey.Enable();

        if (wolfPrefab != null)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);
            return;
        }

        if (wolfAnimator == null)
            wolfAnimator = GetComponentInChildren<Animator>();

        if (wolfMesh == null)
            wolfMesh = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    void OnDestroy() => spaceKey?.Dispose();

    void Update()
    {
        if (wolfPrefab == null || spaceKey == null)
            return;

        if (spaceKey.WasPressedThisFrame())
            SpawnAndLaunch();
    }

    void SpawnAndLaunch()
    {
        if (launchFrom == null || launchTarget == null)
            return;

        WolfAttack wolf = Instantiate(wolfPrefab).GetComponent<WolfAttack>();
        wolf.Launch(
            launchFrom.position,
            launchTarget.position,
            launchErodeTarget,
            fromEffect,
            impactEffect,
            wolfScale,
            wolfDisappearDuration);
    }

    public void Launch(
        Vector3 from,
        Vector3 target,
        SkinnedMeshRenderer targetToErode = null,
        GameObject fromSpawnEffect = null,
        GameObject impact = null,
        Vector3? scale = null,
        float? disappearDuration = null)
    {
        activeErodeTarget = targetToErode != null ? targetToErode : launchErodeTarget;

        if (fromSpawnEffect != null)
            fromEffect = fromSpawnEffect;

        if (impact != null)
            impactEffect = impact;

        if (scale.HasValue)
            wolfScale = scale.Value;

        if (disappearDuration.HasValue)
            wolfDisappearDuration = disappearDuration.Value;

        StartCoroutine(WolfRoutine(from, target));
    }

    IEnumerator WolfRoutine(Vector3 from, Vector3 target)
    {
        SpawnEffect(fromEffect, from);

        transform.SetParent(null);
        transform.position = from;
        transform.localScale = wolfScale;

        Vector3 direction = target - from;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized);

        if (wolfAnimator != null)
            wolfAnimator.Play(runAnimation);

        while (Vector3.Distance(transform.position, target) > arrivalDistance)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            yield return null;
        }

        if (impactEffect != null)
            SpawnEffect(impactEffect, transform.position);

        if (activeErodeTarget != null)
            StartCoroutine(ErodeTarget());

        yield return ErodeRenderer(wolfMesh, wolfDisappearDuration);
        Destroy(gameObject);
    }

    static void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
            return;

        GameObject spawnedEffect = Object.Instantiate(effectPrefab, position, Quaternion.identity);
        foreach (ParticleSystem ps in spawnedEffect.GetComponentsInChildren<ParticleSystem>(true))
            ps.Play(true);
    }

    IEnumerator ErodeRenderer(SkinnedMeshRenderer renderer, float duration)
    {
        if (renderer == null || duration <= 0f)
            yield break;

        renderer.material.SetFloat("_Erode", 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            renderer.material.SetFloat("_Erode", Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        renderer.material.SetFloat("_Erode", 1f);
    }

    IEnumerator ErodeTarget()
    {
        yield return new WaitForSeconds(erodeDelay);

        float t = 0f;
        while (t < 1f)
        {
            t += erodeRate;
            activeErodeTarget.material.SetFloat("_Erode", t);
            yield return new WaitForSeconds(erodeRefreshRate);
        }
    }
}
