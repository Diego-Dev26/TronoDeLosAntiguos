using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TreeEditor;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{   
    public int OpenSide;
    //1 Bot
    //2 Top
    //3 left
    //4 Right

    private RoomTemplate RoomTemplate;
    private int rand;
    private bool Spawned;

    void Start()
    {
        RoomTemplate=GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplate>();
        Invoke("Spawn", 0.1f);
    }
    void Spawn()
    {
        if (Spawned==false)
        {
            if (OpenSide == 1)
            {
                //BOT Door
                rand = Random.Range(0, RoomTemplate.botRooms.Length);
                Instantiate(RoomTemplate.botRooms[rand], transform.position, RoomTemplate.botRooms[rand].transform.rotation);
            }
            else if (OpenSide == 2)
            {
                //TOP Door
                rand = Random.Range(0, RoomTemplate.topRooms.Length);
                Instantiate(RoomTemplate.topRooms[rand], transform.position, RoomTemplate.topRooms[rand].transform.rotation);
            }
            else if (OpenSide == 3)
            {
                //Right Door
                rand = Random.Range(0, RoomTemplate.leftRooms.Length);
                Instantiate(RoomTemplate.leftRooms[rand], transform.position, RoomTemplate.leftRooms[rand].transform.rotation);
                
            }
            else if (OpenSide == 4)
            {
                //Left Door
                rand = Random.Range(0, RoomTemplate.rightRooms.Length);
                Instantiate(RoomTemplate.rightRooms[rand], transform.position, RoomTemplate.rightRooms[rand].transform.rotation);

            }
            Spawned = true;
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpawnPoint"))
        {
            if (RoomTemplate == null)
            {
                GameObject roomsObject = GameObject.FindGameObjectWithTag("Rooms");
                if (roomsObject != null)
                {
                    RoomTemplate = roomsObject.GetComponent<RoomTemplate>();
                }
            }

            RoomSpawner otherSpawner = other.GetComponent<RoomSpawner>();
            if (otherSpawner != null)
            {
                if (!otherSpawner.Spawned && !Spawned)
                {
                    if (RoomTemplate != null && RoomTemplate.ClosedRoom != null)
                    {
                        if (RoomTemplate.CanCloseRoom())
                        {
                            Instantiate(RoomTemplate.ClosedRoom, transform.position, Quaternion.identity);
                            Destroy(gameObject);
                            Spawned = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            Spawned = true;
        }
    }
}
