using System.Collections;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private LayerMask destroyableLayer;

    private void Start()
    {
        // Dinamitin patlama zamaný geldiðinde `ExplodeAfterDelay` coroutine'ini baþlatýr
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        // Çarpýþma alanýnda dinamitin etkilediði nesneleri yok et
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.6f, destroyableLayer);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Barrier"))
            {
                Destroy(collider.gameObject);
            }
        }

        // Dinamit nesnesini yok et
        Destroy(gameObject);
    }
}
