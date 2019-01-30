using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class heroBullet : MonoBehaviour
{
    private float bulletSpeed = 550f;
    // Use this for initialization
    void Start()
    {
        Invoke("Die", 2.1f); //setting lifetime of object to 2.1 seconds
        GetComponent<Rigidbody2D>().velocity= (new Vector3(bulletSpeed * Time.deltaTime, 0));
    }

    // Update is called once per frame

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            GameObject.Destroy(gameObject);
        }
    }
    private void Die()
    {
        GameObject.Destroy(gameObject);
    }
}
