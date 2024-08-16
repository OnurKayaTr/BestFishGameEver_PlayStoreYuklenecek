using UnityEngine;

[CreateAssetMenu(fileName = "FruitObjectSettings", menuName = "Settings/FruitObjectSettings")]
public class FruitObjectSettings : ScriptableObject
{
    public GameObject[] fruitPrefabs;

    public GameObject GetFruitPrefab(int index)
    {
        if (index >= 0 && index < fruitPrefabs.Length)
        {
            return fruitPrefabs[index];
        }

        Debug.LogError("Index is out of range.");
        return null;
    }
    
    public int PrefabCount => fruitPrefabs.Length;
    //ok
}
