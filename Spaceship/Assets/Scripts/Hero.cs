using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero : MonoBehaviour
{
    private int score = 0; //current fitness
    private NeuralNetwork net; //reference to neural network
    private GameStatus gameMaster;
    private GameObject bullet;
    public float speed = 25f; //moving speed
    private bool isAlife = true; 
    private float pointForLivingCooldown = 1f; //how often increase fitness by 1 (default every second)
    private float pflTimeStamp;
    private float[] inputs = new float[5]; //array of input neurons
    private float[] outputs = new float[2]; //array of output neurons
    public float[] Inputs
    {
        get { return inputs; }
    }
    public float[] Outputs
    {
        get { return outputs; }
    }
    private float sensitivity = 0.0f; //how high output value has to be to make hero do certain action (default more than 0)
    private GameObject spawnpoint; 
    private int myID;
    // Use this for initialization
    void Awake()
    {
        //--setting references to objects, finding them on scene--
        spawnpoint = transform.parent.Find("Spawnpoint").gameObject;
        gameMaster = GameObject.Find("SimulationMaster").GetComponent<GameStatus>();
        bullet = (GameObject)Resources.Load("herobullet");
    }
    void Start()
    {
        net = gameMaster.GiveNetworkByID(myID); //getting network reference from gamemaster
        pflTimeStamp = Time.time; //points for living time stamp
        net.SetFitness(0); //initial fitness equals 0
        InvokeRepeating("AIMovement", 0, 0.025f); //propagating inputs values through neural network to get outputs, every 0.025 second
        InvokeRepeating("Shoot", 1, 0.5f); //shooting bullet every 0.5 second
    }

    void Shoot()
    {
        Instantiate(bullet, transform.position, Quaternion.identity);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //if got hit by Enemy
        if (collision.transform.tag == "Enemy" && isAlife ||  collision.transform.tag == "Enemyothr" && isAlife)
        {
            gameObject.layer = 8; //make hero untouchable to enemies
            isAlife = false; 
            EndGame(); //send info of failing
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemyblt" && isAlife) //if got hit by enemy bullet
        {
            gameObject.layer = 8;
            isAlife = false;
            EndGame();
        }
    }

    void EndGame()
    {
        gameMaster.statusButton[myID].GetComponentInChildren<Text>().color = Color.red; //color of score points set to red
        net.SetFitness(score - 7f); //Score lowered by 7 (there's no option of losing before 7 seconds, value is not apparent, used for caluculations only)
        Debug.Log("Fitness sieci: " + (net.GetFitness()));
        gameMaster.reduceLivingCounter(); //send info to gmemaster that one hero just lost
    }

    private GameObject FindClosestEnemy(string tag)
    {
        GameObject[] gos; 
        gos = GameObject.FindGameObjectsWithTag(tag); //array of object with same tag (used for 'enemy' and 'enemyblt')
        GameObject closest = null; //if none found, setting default value to null
        float distance = Mathf.Infinity; //if none found, setting default value to 'infinity'
        Vector3 position = transform.position; //current position of hero
        foreach (GameObject go in gos)
        {
            //--for each gameobject with tag found calculating distance--
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance) //searching minimum at the same time
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
    public bool GetIsAlife()
    {
        return isAlife;
    }
    public void SetMyID(int id)
    {
        myID = id;
    }
    private void AIMovement()
    {
        if (gameMaster.wipeOut) //when area resets
        {
            score = 0;
            isAlife = true;
            inputs[0] = 0f;
            inputs[1] = 0f;
            inputs[2] = 0f;
            inputs[3] = 0f;
            inputs[4] = 0f;
            gameMaster.statusButton[myID].GetComponentInChildren<Text>().color = Color.green;
            net = gameMaster.GiveNetworkByID(myID); //getting network of the next generation with same ID
            gameObject.layer = 0; //making hero touchable to enemies again
            transform.position = spawnpoint.transform.position; //moving hero to fixed reload position
        }
        if (score >= 600)
        {
            //after 10 minutes of survival, game is ended
            isAlife = false;
            EndGame();
        }
        if (isAlife & pflTimeStamp < Time.time)
        {
            //--adding fitness every second--
            score++;
            net.AddFitness(1.0f);
            pflTimeStamp = Time.time + pointForLivingCooldown;
            gameMaster.statusButton[myID].GetComponentInChildren<Text>().text = score.ToString();
        }
        //first input: position y of hero, normalized
        inputs[0] = Mathf.InverseLerp(spawnpoint.transform.position.y - 5f, spawnpoint.transform.position.y + 4.5f, transform.position.y);
        GameObject closest = FindClosestEnemy("Enemy");
        GameObject closest1 = FindClosestEnemy("Enemyblt");
        if (closest != null)
        {
            //relative position on x and y axis of closest enemy ship, normalized
            inputs[1] = Mathf.Clamp(closest.transform.position.x - transform.position.x,-20,20)/ 20;
            inputs[2] = Mathf.Clamp(closest.transform.position.y - transform.position.y,-10,10)/10;
        }
        else
        {
            //if no enemies found
            inputs[1] = 1;
            inputs[2] = 1;
        }
        if (closest1 != null)
        {
            //relative position on x and y axis of closest enemy bullet, normalized
            inputs[3] = Mathf.Clamp(closest1.transform.position.x - transform.position.x, -20, 20) / 20;
            inputs[4] = Mathf.Clamp(closest1.transform.position.y - transform.position.y, -10, 10) / 10;
        }
        else
        {
            //if no enemy bullets found
            inputs[3] = 1;
            inputs[4] = 1;
        }
        outputs = net.FeedForward(inputs); //calculating outputs with fresh inputs
        if (outputs[0] > sensitivity) //moving up when firts output has value>0
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.up * speed * Time.deltaTime;
        }
        if (outputs[1] > sensitivity)//moving down when second output has value>0
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.down * speed * Time.deltaTime;
        }
        if (outputs[0] < sensitivity && outputs[1] < sensitivity) //stopping moving when both outputs are lower than 0
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }
    public int GetScore()
    {
        return score;
    }

}
