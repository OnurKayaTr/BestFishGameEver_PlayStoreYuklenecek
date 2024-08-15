using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Image fillImage;
    [SerializeField] private Image yellowImage;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    private List<FruitObject> activeFruits = new List<FruitObject>();
    private bool isGameOver;
    private int clickCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        if (yellowImage != null)
        {
            yellowImage.gameObject.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
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
        clickCount++;
        UpdateFillAmount();

        if (clickCount % 15 == 0)
        {
            var spawnPosition = new Vector2(GetInputHorizontalPosition(), spawnPoint.position.y);
            SpawnDynamite(spawnPosition);
            ResetFillAmount();

            if (yellowImage != null)
            {
                yellowImage.gameObject.SetActive(false);
            }
        }
        else
        {
            if (Random.value < 0.9f)
            {
                var index = Random.Range(0, Mathf.Min(3, settings.PrefabCount));
                var spawnPosition = new Vector2(GetInputHorizontalPosition(), spawnPoint.position.y);
                SpawnFruit(index, spawnPosition);
            }
            else
            {
                var spawnPosition = new Vector2(GetInputHorizontalPosition(), spawnPoint.position.y);
                SpawnBarrier(spawnPosition);
            }
        }

        if (clickCount % 15 == 14)
        {
            if (yellowImage != null)
            {
                yellowImage.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateFillAmount()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01((float)(clickCount % 15) / 14f);
        }
    }

    private void ResetFillAmount()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
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

    private void SpawnDynamite(Vector2 position)
    {
        if (dynamitePrefab != null)
        {
            Instantiate(dynamitePrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Dynamite prefab is null.");
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

        if (type >= settings.PrefabCount)
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

    // Retry buton iþlevi
    private void RetryGame()
    {
        isGameOver = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Exit buton iþlevi
    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
