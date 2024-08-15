using UnityEngine;

public class FruitObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int type { get; set; } // 'get; set;' kullanarak eriþimi açýyoruz
    public bool SendedMergeSignal { get; set; } // 'get; set;' kullanarak eriþimi açýyoruz
    public float moveSpeed = 2f;
    [SerializeField] private LayerMask barrierLayerMask;

    private void Update()
    {
        MoveTowardsNearestFruit();
    }

    private void MoveTowardsNearestFruit()
    {
        if (!IsCollidingWithBarrier())
        {
            FruitObject nearestFruit = FindNearestSameTypeFruit();
            if (nearestFruit != null)
            {
                Vector2 direction = (nearestFruit.transform.position - transform.position).normalized;
                transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            }
        }
    }

    private bool IsCollidingWithBarrier()
    {
        Collider2D barrierCollision = Physics2D.OverlapCircle(transform.position, 0.1f, barrierLayerMask);
        return barrierCollision != null;
    }

    private FruitObject FindNearestSameTypeFruit()
    {
        FruitObject[] allFruits = FindObjectsOfType<FruitObject>();
        FruitObject nearestFruit = null;
        float minDistance = float.MaxValue;

        foreach (var fruit in allFruits)
        {
            if (fruit != this && fruit.type == type && !fruit.SendedMergeSignal)
            {
                float distance = Vector2.Distance(transform.position, fruit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestFruit = fruit;
                }
            }
        }

        return nearestFruit;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (SendedMergeSignal) return;

        var fruitObj = other.transform.GetComponent<FruitObject>();
        if (fruitObj && fruitObj.type == type && !fruitObj.SendedMergeSignal)
        {
            SendedMergeSignal = true;
            fruitObj.SendedMergeSignal = true;
            GameManager.Instance.Merge(this, fruitObj);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GameOverLine"))
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
