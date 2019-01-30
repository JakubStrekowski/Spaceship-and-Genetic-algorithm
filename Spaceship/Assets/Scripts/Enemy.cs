using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameStatus gameMaster;
    private GameObject bullet;
    private int hp = 3; 
    private float speed = 1.5f;
    private float getinsideTimestamp; //time, during which object will move forward, without aiming not shooting
    private bool gotinside = false; 
    private GameObject target; // hero object, needed for aiming
    private int spawnID; // in which screen object is (0-19)
    Rigidbody2D rb;
    private bool dieonce = false; //used to make Die() method run only once when hp is <=0
    // Use this for initialization
    void Awake()
    {
        //--setting references to objects, finding them on scene--
        target = transform.parent.Find("Hero").gameObject;
        gameMaster = GameObject.Find("SimulationMaster").GetComponent<GameStatus>();
        bullet =(GameObject)Resources.Load("enemybullet");
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        getinsideTimestamp = Time.time + 3f; 
        InvokeRepeating("ShootProjectile",0f,1.8f); //shooting bullets once per 1.8 of second
        rb.velocity = new Vector3(-speed, 0, 0); //moving forward
    }

    // Update is called once per frame
    void Update()
    {
        if (getinsideTimestamp < Time.time&&!gotinside) //after 3 seconds
        {
            rb.velocity = new Vector2(0, 0); //stopping moving
            gotinside = true;
        }
        if (gotinside)
        {
            //--aiming at target--
            Vector3 vectorToTarget = target.transform.position - transform.position; //getting relative position
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg -180; /*getting angle to set
            (sprite of this object is made rotated 180 degrees, so had to do -180 thing at the end)*/
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * speed*4f); 
            //rotating object towards target at the speed of (speed*4)
        }
        if (hp <= 0&&!dieonce) //when object got hit with hero bullets 3 times
        {
            dieonce = true;
            gameMaster.ReduceSpawnCounter(spawnID); //reducing counter of living enemies in spawner, used to set max. enemies at once
            Invoke("Die", 0.1f); //slight delay in destroying object, because destroying instantly could make other methods not run properly
        }
        if (gameMaster.wipeOut)
        {
            Invoke("Die", 0.1f);
        }
    }
    void ShootProjectile()
    {
        if (gotinside) 
        {
            GameObject e = (GameObject)Instantiate(bullet, transform.position, transform.rotation); //creating bullet at the posionion of object, with same rotation
            e.transform.parent = gameObject.transform.parent; //setting same parent of created bullet as creator has (gamewindow)
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "herobullet") //got hit by hero bullet
        {
            hp--;
        }
    }
    public void SetSpawnID(int spawn)
    {
        spawnID = spawn;
    }
    private void Die()
    {
        Destroy(gameObject);
    }
}
