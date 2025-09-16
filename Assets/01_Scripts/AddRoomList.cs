using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRoomList : MonoBehaviour
{
    private RoomTemplate roomTemplate;
    void Start()
    {
        roomTemplate= GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplate>();
        roomTemplate.rooms.Add(this.gameObject);
    }

   
}
