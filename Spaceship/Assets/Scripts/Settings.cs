using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
    private float crossoverRatio;
    private float mutationRatio;
    public InputField mutateRatioIF;
    public InputField crossoverRatioIF;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this.gameObject);
	}

    public void SetCrossOverRatio()
    {
        try
        {
            crossoverRatio = float.Parse(crossoverRatioIF.GetComponent<Text>().text);
            if (crossoverRatio < 0) crossoverRatio = 0.0f;
            if (crossoverRatio > 1f) crossoverRatio = 1f;
        }
        catch{
            crossoverRatio = 0.7f;
        }
    }
    public void SetMutationRatio()
    {
        try
        {
            mutationRatio = float.Parse(mutateRatioIF.GetComponent<Text>().text);
            if (mutationRatio < 0) mutationRatio = 0.0f;
            if (mutationRatio > 1f) mutationRatio = 1f;
        }
        catch
        {
            mutationRatio = 0.01f;
        }
    }


    public float GivemutationRatio()
    {
        return crossoverRatio;
    }
    public float GivecrossoverRatio()
    {
        return mutationRatio;
    }
}
