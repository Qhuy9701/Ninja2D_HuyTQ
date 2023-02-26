using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPickUp : MonoBehaviour
{
    public float healthAmount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

   void OnTrigger2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerHealth>().AddHealth(healthAmount);
            
        }
    }
    
    //roll with y 
    void Update()
    {
        transform.Rotate(new Vector3(0, 90, 0) * Time.deltaTime);
    }
}
