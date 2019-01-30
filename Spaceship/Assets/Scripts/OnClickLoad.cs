using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClickLoad : MonoBehaviour {

	public void LoadNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
