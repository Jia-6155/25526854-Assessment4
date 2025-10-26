using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StartMenuController : MonoBehaviour
{
    public void LoadLevel1()
    {
        StartCoroutine(PlayClickAndLoad("Level1"));
    }
    private IEnumerator PlayClickAndLoad(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }
    public void LoadLevel2(){
        Debug.Log("Level 2 not available yet!");
    }
    public void QuitGame(){
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
