using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

using TMPro;

public class NewGameManager : Singleton<NewGameManager>
{
    public string startTextTag = "Start Text";
    public string endTextTag = "End Text";
    [Header("State control")]
    public int newGameCount = 0;

    [Header("Settings")]
    [Header("Player Settings")]
    public float playerHealthReduction = 0.1f;
    public int minPlayerHealth = 20;

    [Header("Health Pickup Settings")]
    public float healthPickupReduction = 0.1f;
    public int minHealthPickupHeal = 10;

    [Header("Enemy Settings")]    
    public int enemySpeedIncrease = 10;
    public Vector2 randomEnemyRange = new Vector2(10, 40);
    public float minDistanceFromPlayer = 10f;
    public EnemyComponents[] enemyPrefabs;

    public override void Awake()
    {
        base.Awake();
        if (this == Current)
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    public void EnterNewGame()
    {
        newGameCount++;
        MenuButtons.RestartLevel();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected override void OnMultipleInstance()
    {
        Destroy(gameObject);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (newGameCount == 0)
            return;

        SetTextSigns();

        //alter player
        PlayerComponents player = FindObjectOfType<PlayerComponents>();
        //reduce health, same as increasing enemy damage
        int playerHealth = player.characterSheet.GetResourceMax(CharacterSheet.Resource.health);
        playerHealth = ReduceInt(playerHealth, newGameCount * playerHealthReduction);
        if (playerHealth < minPlayerHealth)
            playerHealth = minPlayerHealth;
        player.characterSheet.SetResourceMax(CharacterSheet.Resource.health, playerHealth);

        //alter health pickups
        HealthPickup[] healthPickups = FindObjectsOfType<HealthPickup>();
        for (int i = 0; i < healthPickups.Length; i++)
        {
            healthPickups[i].healAmount = ReduceInt(healthPickups[i].healAmount, newGameCount * healthPickupReduction);
            if (healthPickups[i].healAmount < minHealthPickupHeal)
                healthPickups[i].healAmount = minHealthPickupHeal;
        }

        //alter enemies
        EnemyComponents[] allEnemies = FindObjectsOfType<EnemyComponents>();

        for (int i = 0; i < allEnemies.Length; i++)
            UpgradeEnemy(allEnemies[i]);

        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No enemy prefabs loaded!", this);
            return;
        }

        AISensor[] allSensors = FindObjectsOfType<AISensor>();
        for (int i = 0; i < newGameCount; i++)
            SpawnRandomEnemy(enemyPrefabs[i % enemyPrefabs.Length], player, allEnemies, allSensors);
    }

    private static int ReduceInt(int input, float reduction) => Mathf.FloorToInt((1 - reduction) * input);

    private void SpawnRandomEnemy(EnemyComponents spawnPrefab, PlayerComponents player, EnemyComponents[] allEnemies, AISensor[] allSensors)
    {
        EnemyComponents choosePartner;
        Vector2 randomCircle;
        NavMeshHit hit;

        do
        {
            choosePartner = allEnemies[Random.Range(0, allEnemies.Length - 1)];
            randomCircle = Random.insideUnitCircle.normalized * Random.Range(randomEnemyRange.x, randomEnemyRange.y);
        }
        while (NavMesh.SamplePosition(
            choosePartner.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y),
            out hit, 20f, NavMesh.AllAreas) == false
            ||
            Vector3.Distance(hit.position, player.transform.position) < minDistanceFromPlayer);

        EnemyComponents newEnemy = Instantiate(spawnPrefab, hit.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
        UpgradeEnemy(newEnemy);

        //hook up sensors
        for (int i = 0; i < allSensors.Length; i++)
            if (allSensors[i].affectedEnemies.Contains(choosePartner.enemyControl))
                allSensors[i].affectedEnemies.Add(newEnemy.enemyControl);
    }

    private void UpgradeEnemy(EnemyComponents enemy)
    {
        //increase movement speed
        enemy.characterSheet.IncreaseAttribute(CharacterSheet.Attribute.moveSpeed, newGameCount * enemySpeedIncrease);
    }

    private void SetTextSigns()
    {
        //copy the state
        Random.State copyState = Random.state;
        //i want a specific colour for each new game count
        Random.InitState(newGameCount);
        Color specialColor = Random.ColorHSV();
        //restore the state
        Random.state = copyState;

        GameObject findObject = GameObject.FindGameObjectWithTag(startTextTag);
        findObject.GetComponent<MeshRenderer>().enabled = true;
        var startTMP = findObject.GetComponent<TextMeshPro>();
        startTMP.SetText("Welcome to New Game+" + newGameCount);
        startTMP.color = specialColor;

        findObject = GameObject.FindGameObjectWithTag(endTextTag);
        var endTMP = findObject.GetComponent<TextMeshPro>();
        endTMP.SetText("Finish!\n\nContinue to\nNew Game+" + (newGameCount + 1) + "!");
        endTMP.color = specialColor;
    }
}
