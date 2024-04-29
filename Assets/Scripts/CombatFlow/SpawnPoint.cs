using UnityEngine;
using UnityEngine.Events;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] BotRecord botRecord;
    [SerializeField] Allegiance botAllegiance;
    public static UnityEvent<BotPlacer> ReadyToSpawn = new();
    private void Awake()
    {
        ReadyToSpawn.AddListener(Deploy);
    }

    private void Deploy(BotPlacer placer)
    {
        TinyBot spawnedBot = placer.SpawnBot(botAllegiance, botRecord);
        bool validLocation = Pathfinder3D.GetLandingPointBy(transform.position, spawnedBot.PrimaryMovement.Style, out Vector3Int coords);
        if (!validLocation) Debug.LogError($"Spawner for {botRecord.name} placed at illegal position");
        placer.OrientBot(spawnedBot, coords);
        gameObject.SetActive(false);
    }
}
