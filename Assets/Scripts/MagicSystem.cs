using UnityEngine;
using UnityEngine.SceneManagement;

// Holds the player's magic energy and crystal count.
// Only one MagicSystem can exist; duplicates remove themselves.
public class MagicSystem : MonoBehaviour
{
    public static MagicSystem Instance { get; private set; }

    [SerializeField] private float maxEnergy = 100f;

    public float Energy { get; private set; }
    public float MaxEnergy => maxEnergy;
    public int CrystalsCollected { get; private set; }
    public int TotalCrystals { get; private set; }
    public string Message { get; private set; } = "";
    public bool MessageActive => Time.time < messageEndTime;

    private float messageEndTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        TotalCrystals = 0;
        foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            TotalCrystals += rootObj.GetComponentsInChildren<CollectibleCrystal>().Length;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void CollectCrystal(float energyGain)
    {
        CrystalsCollected++;
        AddEnergy(energyGain);
        PostMessage("Crystal collected! +" + energyGain + " magic energy");
    }

    public void AddEnergy(float amount)
    {
        Energy = Mathf.Clamp(Energy + amount, 0f, maxEnergy);
    }

    public bool TrySpend(float amount)
    {
        if (Energy < amount) return false;
        Energy -= amount;
        return true;
    }

    public void PostMessage(string text, float seconds = 2.5f)
    {
        Message = text;
        messageEndTime = Time.time + seconds;
    }
}
