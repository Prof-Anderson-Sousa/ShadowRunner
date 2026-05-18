using UnityEngine;
using System.Collections;

public class MorcegoSpawner : MonoBehaviour
{
    [SerializeField] private GameObject morcegoPrefab;
    [SerializeField] private float intervaloInicial = 5f;
    [SerializeField] private int maxMorcegosInicial = 5;

    [SerializeField] private float xSpawn = 12f;
    [SerializeField] private float ySpawnCima = 3f;
    [SerializeField] private float ySpawnBaixo = 0f;

    private float intervaloAtual;
    private int maxMorcegosAtual;
    private int ondaAtual = 1;
    private int morcegosSpawnadosNaOnda;
    private bool ondaEmAndamento;

    void Start()
    {
        intervaloAtual = intervaloInicial;
        maxMorcegosAtual = maxMorcegosInicial;
        GameManager.Instance?.RegistrarNovaOnda(ondaAtual);
        IniciarNovaOnda();
        StartCoroutine(LoopDeSpawn());
    }

    IEnumerator LoopDeSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloAtual);

            int morcegosAtivos = GameObject.FindGameObjectsWithTag("Inimigo").Length;

            bool todosDaOndaForamSpawnados = morcegosSpawnadosNaOnda >= maxMorcegosAtual;

            if (ondaEmAndamento && todosDaOndaForamSpawnados && morcegosAtivos == 0)
            {
                ondaEmAndamento = false;
                ondaAtual++;
                maxMorcegosAtual += 2;
                intervaloAtual = Mathf.Max(0.8f, intervaloAtual - 0.5f);
                GameManager.Instance?.RegistrarNovaOnda(ondaAtual);
                IniciarNovaOnda();
                Debug.Log($"Onda {ondaAtual}! Max morcegos: {maxMorcegosAtual}");
            }

            if (ondaEmAndamento && morcegosSpawnadosNaOnda < maxMorcegosAtual)
            {
                float yEscolhido = (morcegosAtivos % 2 == 0) ? ySpawnCima : ySpawnBaixo;
                Vector3 posicao = new Vector3(xSpawn, yEscolhido, 0f);
                Instantiate(morcegoPrefab, posicao, Quaternion.identity);
                morcegosSpawnadosNaOnda++;
            }

            float tempo = GameManager.Instance.TempoDejogo;
            intervaloAtual = Mathf.Max(0.8f, intervaloInicial - tempo * 0.02f);
        }
    }

    void IniciarNovaOnda()
    {
        morcegosSpawnadosNaOnda = 0;
        ondaEmAndamento = true;
    }
}
