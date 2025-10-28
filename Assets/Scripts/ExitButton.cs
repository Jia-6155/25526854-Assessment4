using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitButton : MonoBehaviour
{
    public void BackToStart() {
        SceneManager.LoadScene("StartScene");
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
