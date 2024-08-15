using System.Collections;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private LayerMask destroyableLayer;

    private void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, destroyableLayer);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("Barrier"))
            {
                Destroy(collider.gameObject);
            }
        }

        Destroy(gameObject);
    }
}
