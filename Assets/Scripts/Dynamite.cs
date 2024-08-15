using System.Collections;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f; // Dinamit patlama s�resi
    [SerializeField] private LayerMask destroyableLayer; // Yok edilecek objeler i�in LayerMask

    private void Start()
    {
        // Patlama s�resi sonunda yok olma i�lemini ba�lat
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        // Dinamit patlad���nda etkiledi�i objeleri yok et
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, destroyableLayer);
        foreach (Collider2D collider in colliders)
        {
            // Objelerin tag'lerini kontrol et ve sadece 'Barrier' tag'ine sahip olmayanlar� yok et
            if (!collider.CompareTag("Barrier"))
            {
                Destroy(collider.gameObject);
            }
        }

        // Dinamiti yok et
        Destroy(gameObject);
    }
}
