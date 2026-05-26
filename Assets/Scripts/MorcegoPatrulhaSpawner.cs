using UnityEngine;

public class MorcegoPatrulhaSpawner : MonoBehaviour
{
    [SerializeField] private GameObject morcegoPatrulhaPrefab;
    [SerializeField] private Transform pontoMorcego1;
    [SerializeField] private Transform pontoMorcego2;
    [SerializeField] private Transform pontoMorcego3;
    [SerializeField] private Transform pontoMorcego4;

    private void Start()
    {
        if (morcegoPatrulhaPrefab == null)
        {
            Debug.LogError("MorcegoPatrulhaSpawner: prefab do morcego patrulha não foi atribuído.");
            return;
        }

        CriarMorcego(pontoMorcego1);
        CriarMorcego(pontoMorcego2);
        CriarMorcego(pontoMorcego3);
        CriarMorcego(pontoMorcego4);
    }

    private void CriarMorcego(Transform ponto)
    {
        if (ponto == null)
            return;

        Instantiate(morcegoPatrulhaPrefab, ponto.position, Quaternion.identity);
    }
}
