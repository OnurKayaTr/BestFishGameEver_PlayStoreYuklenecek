using System.Collections;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f; // Dinamit patlama süresi
    [SerializeField] private LayerMask destroyableLayer; // Yok edilecek objeler için LayerMask

    private void Start()
    {
        // Patlama süresi sonunda yok olma iþlemini baþlat
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        // Dinamit patladýðýnda etkilediði objeleri yok et
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, destroyableLayer);
        foreach (Collider2D collider in colliders)
        {
            // Objelerin tag'lerini kontrol et ve sadece 'Barrier' tag'ine sahip olmayanlarý yok et
            if (!collider.CompareTag("Barrier"))
            {
                Destroy(collider.gameObject);
            }
        }

        // Dinamiti yok et
        Destroy(gameObject);
    }
}
