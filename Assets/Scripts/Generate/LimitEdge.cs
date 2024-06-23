using System.Collections;
using UnityEngine;

public class LimitEdge : MonoBehaviour
{
    public GameObject player;
    public GameObject terrain;

    public void Update()
    {
        if (player.GetComponent<CapsuleCollider2D>().IsTouching(terrain.GetComponent<BoxCollider2D>()))
        {
            player.transform.position = new Vector3(player.transform.position.x, terrain.transform.position.y + player.GetComponent<Collider>().bounds.size.y / 2, player.transform.position.z);
        }
    }
}
