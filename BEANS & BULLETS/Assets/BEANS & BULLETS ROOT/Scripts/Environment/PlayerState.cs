using NUnit;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static PlayerState Instance;
{
 void Awake ()
  {
  Instance = this;
  }

 private void SaveState()
{
    Vector3 rootPos = TransitionRoot.Instance.transform.position;
    Vector3 posDif = PlayerMovement.Instance.transform.position - rootPos;
    position = posDif;
    rotation = PlayerMovement.Instance.GetPlayerCamTransform().rotation;
    velocity = PlayerMovement.Instance.GetVelocity();
    mouseOffset = PlayerInput.Instance.GetMouseOffset();

    //Debug lines
    UnityEngine.Debug.DrawLine(start rootPos,end rootPos + position, Color.red, duration 10f);
}

public void LoadState()
{
    PlayerMovement.Instance.transform.position = TransitionRoot.Instance.transofom.position + position;
    PlayerMovement.Instance.GetPlayerCamTransform().rotation = rotation;
    PlayerMovement.Instance.GetRb().velocity = velocity;
    PlayerInput.Instance.SetMouseOffset(mouseOffset);

    // Debug lines
    Vector3 rootPos = TransitionRoot.Instance.Transform.position;
    UnityEngine.Debug.DrawLine(start rootPos, end rootPos + position, Color.red, duration 10f);
}

void Update () 
    {
        if (Input.GetKeyDown(KeyCode.F))
    {
        //Save state of player 
        SaveState();

        // Load next scene in the background
        StartCoroutine(routine LoadYourAsyncScene("Test2"));
    }

IEnumerator LoadYourAsyncScene(string sceneName)
    {
        AsyncOperation AO = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        AO.allowSceneActivation = false;

        while (AO.progress < 0.9f)
        {
            yield return null;
        }

        // Allow scene activation aganin, otherwise we'll be stuck in void
        AO.allowSceneActivation = true;
    }

}
