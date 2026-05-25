using UnityEngine;

public class MorcegoSpawner : MonoBehaviour
{
    [SerializeField] private GameObject morcegoPrefab;
    [SerializeField] private Vector2 posicaoInicial = new Vector2(0f, 1.5f);

    void Start()
    {
        GameManager.Instance?.RegistrarNovaOnda(1);

        if (morcegoPrefab != null)
            Instantiate(morcegoPrefab, posicaoInicial, Quaternion.identity);
    }
}
