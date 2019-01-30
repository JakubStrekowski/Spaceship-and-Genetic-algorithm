using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour {
    private GameStatus gameMaster;
    private float bulletSpeed = 7.4f;
    // Use this for initialization
    void Awake()
    {
        gameMaster = GameObject.Find("SimulationMaster").GetComponent<GameStatus>(); //finding master object
    }
    void Start () {
        Invoke("Die", 5.7f); // set time limit of living
        GetComponent<Rigidbody2D>().velocity=  -transform.right*bulletSpeed; //set velocity to left vector
    }
	
	// Update is called once per frame
	void Update () {
        if (gameMaster.wipeOut) //in case of reloading area
        {
            Die();
        }
    }
    void Die()
    {
        GameObject.Destroy(gameObject);
    }
}
