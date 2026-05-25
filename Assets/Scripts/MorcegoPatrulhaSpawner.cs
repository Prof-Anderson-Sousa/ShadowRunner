using UnityEngine;

public class MorcegoPatrulhaSpawner : MonoBehaviour
{
    [SerializeField] private GameObject morcegoPatrulhaPrefab;
    [SerializeField] private Transform[] pontosSpawn;

    private void Start()
    {
        if (morcegoPatrulhaPrefab == null)
        {
            Debug.LogError("MorcegoPatrulhaSpawner: prefab do morcego patrulha não foi atribuído.");
            return;
        }

        if (pontosSpawn == null || pontosSpawn.Length == 0)
        {
            Debug.LogWarning("MorcegoPatrulhaSpawner: nenhum ponto de spawn foi configurado.");
            return;
        }

        foreach (Transform ponto in pontosSpawn)
        {
            if (ponto == null)
                continue;

            Instantiate(morcegoPatrulhaPrefab, ponto.position, Quaternion.identity);
        }
    }
}