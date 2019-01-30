using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameStatus : MonoBehaviour {
    private Settings settings;
    private Spawner[] spawnerList=new Spawner[20]; 
    public Button[] statusButton=new Button[20];
    private GameObject[] gameWindows = new GameObject[20];
    private Text inputsTxt; 
    public bool wipeOut = false; //should area be reloaded
    private int generationNumber = 1; //number of generation
    private float wipeOutCooldown=2f; 
    private float wipeOutTImeStamp;
    private Text gener;
    private int[] layers = new int[] { 5, 4,20,30,10 ,4 }; //defining quantity of layers and how many neurons are in each layer (first has to be 5, last:2)
    private List<NeuralNetwork> Population; //list of current population
    public NeuralNetwork bestnw; //best nerwork
    private Text besttxt;
    private Text currentFitnessTxt;
    private int populationSize = 20; 
    private float crossoverRatio=0.7f;
    private float mutationRatio = 0.01f;
    private int howManyBests = 10; //how many networks are used as potential parents (default best 10)
    private int livingCounter = 20; //counting living heroes
    private bool zoomedIn = false; //is screen zoomed in
    //selected camera and hero when screen is zoomed:
    Camera cam;
    private float camx;
    private float camy;
    private Hero selectedHero;
    // Use this for initialization
    void Awake()
    {
        settings = GameObject.Find("settings").GetComponent<Settings>();
        //--setting references to objects, finding them on scene--
        List<Spawner>spList = new List<Spawner>();
        for(int i = 0; i < 20; i++)
        {
            spList.Add(GameObject.Find("Spawner"+i.ToString()).GetComponent<Spawner>());
            statusButton[i] = GameObject.Find("speciebtn_" + i.ToString()).GetComponent<Button>();
            gameWindows[i] = GameObject.Find("GameWindow" + i.ToString());
        }
        spawnerList = spList.ToArray();
        for(int i = 0; i < 20; i++)
        {
            spawnerList[i].spawnID = i;
        }
        Population = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            Population.Add(new NeuralNetwork(layers));
            Population[i].SetMutationRate(mutationRatio);
        }
        gener = GameObject.Find("generation_txt").GetComponent<Text>();
        besttxt= GameObject.Find("best_txt").GetComponent<Text>();
        inputsTxt = GameObject.Find("inputs_txt").GetComponent<Text>();
        currentFitnessTxt = GameObject.Find("currentFitness_txt").GetComponent<Text>();
    }
	void Start () {
        crossoverRatio = settings.GivecrossoverRatio();
        mutationRatio = settings.GivemutationRatio();
        inputsTxt.enabled = false; //hiding part of gui
        currentFitnessTxt.enabled = false; //hiding part of gui
        besttxt.text = "Najlepszy wynik: 0 G: 0"; 
        gener.text = "Generacja: " + generationNumber;
    }
	// Update is called once per frame
	void Update () {
        if (zoomedIn) 
        { 
            //preparing gui when zoomed in
            WriteInputOnScreen();
            if (selectedHero.GetIsAlife())
            {
                currentFitnessTxt.color = Color.green;
            }
            else
            {
                currentFitnessTxt.color = Color.red;
            }
            currentFitnessTxt.text = selectedHero.GetScore().ToString();
        }
        if (zoomedIn && Input.GetAxis("Cancel")>0){ //canceling zoom with 'esc'
            ReturnNormalView();
        }

        if (wipeOut&wipeOutTImeStamp<Time.time)
        {
            wipeOut = false;
        }
        if (livingCounter == 0) //when every hero died
        {
            RestartGame();
        }
	}
    public void RestartGame() //preparing another generation and counters
    {
        StartNewPopulation(); //creating new neural networks
        gener.text = "Generacja: " + generationNumber;
        wipeOut = true;
        wipeOutTImeStamp = Time.time + wipeOutCooldown;
        livingCounter = 20;
    }
    public void WriteInputOnScreen() //writing input and output values of neuron network
    {
        if (selectedHero != null)
        {
            string s = "I: ";
            foreach (float i in selectedHero.Inputs)
            {
                s += i.ToString() + " ";
            }
            s += "O: " + selectedHero.Outputs[0].ToString() + " " + selectedHero.Outputs[1].ToString();
            inputsTxt.text = s;
        }
    }
    public void AddToPopulation(NeuralNetwork nw)
    {
        Population.Add(nw);
    }
    public void StartNewPopulation() {
        generationNumber++;
        List<NeuralNetwork> OrderedPopulation = new List<NeuralNetwork>(Population.OrderByDescending(x => x.GetFitness()));/*
        Ordering population descending by fitness*/
        if (bestnw != null)
        {
            //checking if new best network is better than best of all time
            if (OrderedPopulation[0].GetFitness() >= bestnw.GetFitness())
            {
                bestnw = new NeuralNetwork(OrderedPopulation[0]);
                besttxt.text = "Najlepszy wynik: " + (bestnw.GetFitness()+7f) + " G: " + (generationNumber - 1);
            }
        }
        else
        {
            //when there was no best before
            bestnw = OrderedPopulation[0];
            besttxt.text = "Najlepszy wynik: " + (bestnw.GetFitness() + 7f) + " G: " + (generationNumber-1);
        }
        List<NeuralNetwork> newPopulation = new List<NeuralNetwork>();
        int first, second; //first and second parent
        newPopulation.Add(new NeuralNetwork(OrderedPopulation[0])); //elitism = 1, best network moved without modifications
        newPopulation[0].SetMutationRate(mutationRatio);
        for (int i = 1; i < 20; i++)
        {
            float rndm = Random.Range(0.0f, 1.0f);
            if (crossoverRatio < rndm)
            {
                //--setting normalized fitness value, checking if first!=second--
                float[] normalizedFitness = DoNormalizedFitness();
                first = ChooseParentRoulette(normalizedFitness);
                do
                {
                    second = ChooseParentRoulette(normalizedFitness);
                } while (first == second);
                //crossover of selected parents and mutating child
                newPopulation.Add(new NeuralNetwork(OrderedPopulation[first].CrossOver(OrderedPopulation[second])));
                newPopulation[i].SetMutationRate(mutationRatio);
                newPopulation[i].Mutate();
            }
            else
            {
                newPopulation.Add(new NeuralNetwork(layers));
                newPopulation[i].SetMutationRate(mutationRatio);
            }
        }
        Population = new List<NeuralNetwork>(newPopulation); //copying values
    }
    private int ChooseParentRoulette(float [] normalizedFit) //roulette algorithm of selecting parents
    {
        float random = Random.Range(0.0f, 1.0f);
        for (int j = 0; j < howManyBests; j++)
        {
            if (normalizedFit[j] >= random)
            {
                return j;
            }
            else
            {
                continue;
            }
        }
        return howManyBests;
    }

    private float[] DoNormalizedFitness() //counting normalized fitness value for part of population
    {
        float totalFitness=0;
        for(int j = 0; j < howManyBests; j++)
        {
            totalFitness += Population[j].GetFitness();
        }
        float[] FitnessNormalized = new float[howManyBests];
        FitnessNormalized[0] = (float)Population[0].GetFitness() / totalFitness;
        for (int j=1; j < howManyBests; j++)
        {
            FitnessNormalized[j] = FitnessNormalized[j-1]+ Population[j].GetFitness() / totalFitness;
        }
        return FitnessNormalized;
    }
    public NeuralNetwork GiveNetworkByID(int id)
    {
        return Population[id];
    }
    public void ReduceSpawnCounter(int spawnID)
    {
        spawnerList[spawnID].LowerSpawnCounter();
    }
    public void reduceLivingCounter()
    {
        livingCounter--;
    }
    public int GetLivingCounter()
    {
        return livingCounter;
    }
    public void SwitchView(int buttonID) //zooming in
    {
        currentFitnessTxt.enabled = true;
        selectedHero = gameWindows[buttonID].GetComponentInChildren<Hero>();
        zoomedIn = true;
        cam=gameWindows[buttonID].GetComponentInChildren<Camera>();
        cam.transform.GetChild(1).transform.localScale = new Vector3(2.5f, 1.411922f); //increasing size of a frame of selected camera
        for(int i = 0; i < 20; i++) //disabling buttons and hiding them
        {
            statusButton[i].enabled=false;
            statusButton[i].GetComponentInChildren<Text>().enabled = false;
        }
        inputsTxt.enabled = true;
        camx = cam.rect.x;
        camy = cam.rect.y;
        cam.rect = new Rect(0, 0, 1, 1); //increasing size of camera
        cam.depth++; //making camera by 'on top' of others
    }
    public void ReturnNormalView() //zooming out, basically reversing operations of SwitchView()
    {
        currentFitnessTxt.enabled = false;
        for (int i = 0; i < 20; i++)
        {
            statusButton[i].enabled = true;
            statusButton[i].GetComponentInChildren<Text>().enabled = true;
        }
        zoomedIn = false;
        inputsTxt.enabled = false;
        cam.rect = new Rect(camx, camy, 0.2f, 0.25f);
        cam.depth--;
        cam.transform.GetChild(1).transform.localScale = new Vector3(2.1f, 1.411922f);
    }
}
