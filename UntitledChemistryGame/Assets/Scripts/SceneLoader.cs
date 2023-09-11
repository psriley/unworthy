using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Required")]
    public GameObject boat;
    public GameObject sea;
    public GameObject boatCam;
    public GameObject terrain;

    public AudioSource knock;

    private GameManager gm;

    private void Start()
    {
        gm = GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if scene is IP2, then change dock event dependency dependent dialogue to "not creative enough"
        if (scene.name == "[f]IP2")
        {
            Debug.Log("THIS IS THE SCENE!");
            gm.canFish = true;
            gm.lockBook = false;
        }

        //Debug.Log(scene.name);
        //if (scene.name == "[r]AlarmClock")
        //{
        //    // play door knocking sound
        //    Debug.Log("KNOCK KNOCK KNOCK!!!");
        //    knock.Play();
        //}

        // This function will be called every time a scene is loaded.
        if (scene.name[1] == 'f')
        {
            // show fishing specific objects
            boat.SetActive(true);
            sea.SetActive(true);
            boatCam.SetActive(true);
            terrain.SetActive(true);
        }
        else if (scene.name[1] == 'r')
        {
            // hide fishing specific objects
            boat.SetActive(false);
            sea.SetActive(false);
            boatCam.SetActive(false);
            terrain.SetActive(false);
        }
    }
}