using UnityEngine;
public class DustEffect : MonoBehaviour
{
    // Tempo que a animação da poeira dura na tela antes de sumir
    // Tempo que a animação da poeira dura na tela antes de sumir
    public float lifetime = 0.4f; 

    void Start()
    {
        // O Unity destrói este objeto automaticamente após os segundos definidos
        // O Unity destrói este objeto automaticamente após os segundos definidos
        Destroy(gameObject, lifetime);

    }
}
