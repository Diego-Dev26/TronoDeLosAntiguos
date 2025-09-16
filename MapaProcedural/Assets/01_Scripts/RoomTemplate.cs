using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplate : MonoBehaviour
{
    public GameObject[] botRooms;
    public GameObject[] topRooms;
    public GameObject[] leftRooms;
    public GameObject[] rightRooms;
    public GameObject ClosedRoom;
    public List<GameObject> rooms;

    public GameObject boss;
    public GameObject enemy;
    public GameObject shopPrefab;
    public int shopIndexFromEnd = 2;
    public float shopYOffset = 0.1f;

    public int minRooms = 5;
    private void Start()
    {
        Invoke("SpawnShop", 2.5f);
        Invoke("SpawnEnemy", 3f);
    }
    public bool CanCloseRoom()
    {
        return rooms != null && rooms.Count >= minRooms;
    }
    void SpawnEnemy()
    {
        Instantiate(boss, rooms[rooms.Count - 1].transform.position, Quaternion.identity);
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Instantiate(enemy, rooms[i].transform.position, Quaternion.identity);
        }
    }

    void SpawnShop()
    {
        if (shopPrefab == null || rooms == null || rooms.Count < 3) return;
        int targetIndex = rooms.Count - shopIndexFromEnd;
        targetIndex = Mathf.Clamp(targetIndex, 2, rooms.Count - 2);
        Vector3 pos = rooms[targetIndex].transform.position + Vector3.up * shopYOffset;
        Instantiate(shopPrefab, pos, Quaternion.identity);
    }
}
