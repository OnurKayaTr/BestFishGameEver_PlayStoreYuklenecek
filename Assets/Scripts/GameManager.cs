using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private FruitObjectSettings settings;
    [SerializeField] private GameArea gameArea;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Transform gameOverLine;
    [SerializeField] private float gameOverDelay = 1f;
    [SerializeField] private GameObject[] barrierPrefabs;
    [SerializeField] private GameObject dynamitePrefab;

    private List<FruitObject> activeFruits = new List<FruitObject>();
    private bool isGameOver;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isGameOver)
        {
            OnClick();
        }

        MoveFruitsTowardsClosest();
    }

    private void OnClick()
    {
        if (Random.value < 0.9f) // Meyve spawn olma olasýlýðý %50
        {
            var index = Random.Range(0, Mathf.Min(3, settings.PrefabCount)); // Ýlk 3 türden spawn yap
            var spawnPosition = new Vector2(GetInputHorizontalPosition(), spawnPoint.position.y);
            SpawnFruit(index, spawnPosition);
        }
        else
        {
            var spawnPosition = new Vector2(GetInputHorizontalPosition(), spawnPoint.position.y);
            SpawnBarrier(spawnPosition);
        }
    }

    private float GetInputHorizontalPosition()
    {
        var inputPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        var limit = gameArea.GetBorderPosHorizontal();
        return Mathf.Clamp(inputPosition, limit.x, limit.y);
    }

    private FruitObject SpawnFruit(int index, Vector2 position)
    {
        var prefab = settings.GetFruitPrefab(index);
        if (prefab != null)
        {
            var fruitObj = Instantiate(prefab, position, Quaternion.identity).GetComponent<FruitObject>();
            fruitObj.type = index;
            activeFruits.Add(fruitObj);
            return fruitObj;
        }
        else
        {
            Debug.LogError("Prefab is null for index: " + index);
            return null;
        }
    }

    private void SpawnBarrier(Vector2 position)
    {
        int randomIndex = Random.Range(0, barrierPrefabs.Length);
        var barrierPrefab = barrierPrefabs[randomIndex];

        if (barrierPrefab != null)
        {
            Instantiate(barrierPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Barrier prefab is null.");
        }
    }

    private void MoveFruitsTowardsClosest()
    {
        foreach (var fruit in activeFruits)
        {
            if (fruit == null) continue;

            FruitObject closestFruit = FindClosestFruit(fruit);
            if (closestFruit != null)
            {
                Vector2 direction = (closestFruit.transform.position - fruit.transform.position).normalized;
                fruit.transform.position = Vector2.MoveTowards(fruit.transform.position, closestFruit.transform.position, Time.deltaTime);
            }
        }
    }

    private FruitObject FindClosestFruit(FruitObject currentFruit)
    {
        FruitObject closestFruit = null;
        float closestDistance = float.MaxValue;

        foreach (var fruit in activeFruits)
        {
            if (fruit == null || fruit == currentFruit || fruit.type != currentFruit.type) continue;

            float distance = Vector2.Distance(currentFruit.transform.position, fruit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFruit = fruit;
            }
        }

        return closestFruit;
    }

    public void Merge(FruitObject first, FruitObject second)
    {
        var type = first.type + 1;
        var spawnPosition = (first.transform.position + second.transform.position) / 2f;

        if (type >= settings.PrefabCount) // Eðer tür mevcut deðilse, oyunu bitir
        {
            TriggerGameOver();
            return;
        }

        Destroy(first.gameObject);
        Destroy(second.gameObject);

        var newFruit = SpawnFruit(type, spawnPosition);

        if (newFruit != null)
        {
            StartCoroutine(ResetMergeFlag(newFruit));
        }
    }

    private IEnumerator ResetMergeFlag(FruitObject fruit)
    {
        yield return new WaitForSeconds(0.1f);
        fruit.SendedMergeSignal = false;
    }

    public void TriggerGameOver()
    {
        if (!isGameOver)
        {
            StartCoroutine(GameOverSequence());
        }
    }

    private IEnumerator GameOverSequence()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        yield return new WaitForSeconds(gameOverDelay);
        Time.timeScale = 0;
        Debug.Log("Game Over Triggered");
    }

    public void CheckGameOver(FruitObject fruit)
    {
        if (fruit.transform.position.y >= gameOverLine.position.y)
        {
            TriggerGameOver();
        }
    }
}
