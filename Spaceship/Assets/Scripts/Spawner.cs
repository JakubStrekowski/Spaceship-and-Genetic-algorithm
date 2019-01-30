using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    private GameStatus gameMaster;
    private GameObject enemy;
    private Hero hero;
    private float spawnCooldown=6f; //time between spawning enemies
    private int spawnCounter0 = 0; //counting living enemies
    public int spawnID; // in which screen object is (0-19)
    // Use this for initialization
    void Awake()
    {
        //--setting references to objects, finding them on scene--
        hero = transform.parent.Find("Hero").GetComponent<Hero>();
        gameMaster = GameObject.Find("SimulationMaster").GetComponent<GameStatus>();
    }
	void Start () {
        hero.SetMyID(spawnID); //setting id of hero same as this
        enemy = (GameObject)Resources.Load("Enemy2"); //loading resource from file "Assets/Resources"
        InvokeRepeating("SpawnEnemy", 1f, spawnCooldown); //repeating method SpawnEnemy() every (spawnCooldown) seconds, after one second
	}
	// Update is called once per frame
	void Update () {
        if (gameMaster.wipeOut) //when world reloads (all heroes lost)
        {
            spawnCooldown = 6;
            spawnCounter0 = 0;
        }
	}
    public void LowerSpawnCounter() //lowering counter, used when enemy dies to bullet
    {
        spawnCounter0--;
    }
    private void SpawnEnemy()
    {
        if (spawnCounter0 < 5 && hero.GetIsAlife()) //if there are less than 5 enemies, and hero is alife
        {
            //--creating object at the posiitiong of object, -+random Y value, then setting same ID of screen as object has, then incrementing counter--
            GameObject e=(GameObject)Instantiate(enemy, new Vector3(transform.position.x, transform.position.y + Random.Range(-4f, 4f)), Quaternion.identity, gameObject.transform.parent);
            e.GetComponent<Enemy>().SetSpawnID(spawnID);
            spawnCounter0++;
        }
    }
}
