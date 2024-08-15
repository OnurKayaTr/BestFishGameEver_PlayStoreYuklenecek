using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int type { get; private set; }
    public bool SendedMergeSignal { get; private set; }
    public float moveSpeed = 2f; // Meyvelerin hareket hýzý
    [SerializeField] private LayerMask barrierLayerMask; // Bariyerleri tanýmlamak için bir LayerMask

    public void Prepare(Sprite sprite, int index, float scale)
    {
        spriteRenderer.sprite = sprite;
        type = index;
        transform.localScale = Vector3.one * scale;
    }

    private void Update()
    {
        MoveTowardsNearestFruit();
    }

    private void MoveTowardsNearestFruit()
    {
        // Sadece meyvelerin bariyerlere çarpmadan birbirine gitmesi
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
        // Bariyerle çarpýþmayý kontrol et
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
        // Bariyer objesiyle çarpýþýyorsa iþlem yapma
        if (other.gameObject.layer == LayerMask.NameToLayer("Barrier")) return;

        var fruitObj = other.transform.GetComponent<FruitObject>();

        if (!fruitObj) { return; }
        if (fruitObj.type != type) { return; }
        if (fruitObj.SendedMergeSignal) { return; }
        SendedMergeSignal = true;
        GameManager.Instance.Merge(this, fruitObj);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Bariyer objesiyle çarpýþýyorsa iþlem yapma
        if (other.gameObject.layer == LayerMask.NameToLayer("Barrier")) return;

        if (other.CompareTag("GameOverLine"))
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            var fruitObj = other.GetComponent<FruitObject>();
            if (!fruitObj) { return; }
            if (fruitObj.type != type) { return; }
            if (fruitObj.SendedMergeSignal) { return; }
            SendedMergeSignal = true;
            GameManager.Instance.Merge(this, fruitObj);
        }
    }
}
