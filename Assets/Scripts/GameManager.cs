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
    [SerializeField] private Transform gameOverLine; // Çizgi objesi
    [SerializeField] private float gameOverDelay = 1f;
    [SerializeField] private GameObject[] barrierPrefabs; // Bariyer prefablari
     

    // Dinamit prefabýný bariyer prefablarýna ekleyin
    [SerializeField] private GameObject dynamitePrefab;

    private List<FruitObject> activeFruits = new List<FruitObject>();
    private bool isGameOver;
    private bool IsClick => Input.GetMouseButtonDown(0);
    private readonly Vector2Int fruitRange = new Vector2Int(0, 4);

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (IsClick && !isGameOver)
        {
            OnClick();
        }

        MoveFruitsTowardsClosest();
    }

    private void OnClick()
    {
        // Rastgele bir seçim yaparak ya meyve ya da bariyer spawn edeceðiz
        if (Random.value < 0.7f) // Burada meyve spawn olma olasýlýðý %50
        {
            var index = Random.Range(fruitRange.x, fruitRange.y);
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

    private void SpawnFruit(int index, Vector2 position)
    {
        var prefab = settings.SpawnObject;
        var fruitObj = Instantiate(prefab, position, Quaternion.identity);
        var sprite = settings.GetSprite(index);
        var scale = settings.GetScale(index);

        fruitObj.Prepare(sprite, index, scale);
        activeFruits.Add(fruitObj);
    }

   
    private void SpawnBarrier(Vector2 position)
    {
        int randomIndex = Random.Range(0, barrierPrefabs.Length);
        var barrierPrefab = barrierPrefabs[randomIndex];

        if (barrierPrefab == dynamitePrefab)
        {
            Instantiate(barrierPrefab, position, Quaternion.identity);
        }
        else
        {
            Instantiate(barrierPrefab, position, Quaternion.identity);
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
        Destroy(first.gameObject);
        Destroy(second.gameObject);
        SpawnFruit(type, spawnPosition);
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
