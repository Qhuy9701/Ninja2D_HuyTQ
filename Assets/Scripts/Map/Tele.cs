using UnityEngine;

public class Tele : MonoBehaviour
{
    public Transform destination;

    private void OnMouseDown()
    {
        TeleportToDestination();
    }

    public void TeleportToDestination()
    {
        //teleport player position to the remaining point
        GameObject.FindGameObjectWithTag("Player").transform.position = destination.position;   
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            TeleportToDestination();
        }
    }
}
