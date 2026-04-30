using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Botones")]
    public Button[] menuButtons;
    public TextMeshProUGUI[] buttonTexts;

    [Header("Panel de descripción")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI infoText; // Texto inicial cuando nada está seleccionado

    [Header("Descripciones por botón")]
    public string[] descriptions = {
        "> Accessing combat module...\r\n> Loading enemy AI........OK\r\n> Ammo reserves..........OK\r\n> WARNING: Bean count critical\r\n>\r\n> You better be quick.\r\n> And precise.\r\n>\r\n> > Press [E] to deploy _",
        "> Accessing system config...\r\n> Display adapter.......OK\r\n> Audio drivers.........OK\r\n> Input mapping.........OK\r\n>\r\n> What do you need to change?\r\n> The operating system???\r\n>\r\n> > Press [E] to DO NOTHING _",
        "> Initiating shutdown...\r\n> Saving user data......OK\r\n> Closing processes.....OK\r\n>\r\n> Come on.\r\n> They're just beans.\r\n> It isn't that big of a deal.\r\n>\r\n> > Press [E] to confirm _"
    };

    [Header("Colores")]
    public Color selectedBG = Color.white;
    public Color normalBG = Color.clear;
    public Color selectedText = Color.black;
    public Color normalText = Color.white;

    [Header("Audio")]
    public AudioSource bgMusic;
    public AudioClip navigateSound;
    public AudioClip confirmSound;
    private AudioSource sfxSource;

    private int currentIndex = -1; // -1 = nada seleccionado
    private bool menuActive = false;

    void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();

        if (bgMusic != null) bgMusic.Play();

        // Estado inicial: nada seleccionado
        SetAllNormal();
        descriptionText.text = "";
        infoText.gameObject.SetActive(true);
    }

    void Update()
    {
        float v = Input.GetAxisRaw("Vertical");

        if (!menuActive && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            menuActive = true;
            infoText.gameObject.SetActive(false);
            SetIndex(0);
            return;
        }

        if (menuActive)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                SetIndex((currentIndex + 1) % menuButtons.Length);
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                SetIndex((currentIndex - 1 + menuButtons.Length) % menuButtons.Length);

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                ConfirmSelection();
        }
    }

    void SetIndex(int index)
    {
        SetAllNormal();
        currentIndex = index;

        // Resaltar seleccionado
        var colors = menuButtons[index].colors;
        colors.normalColor = selectedBG;
        menuButtons[index].colors = colors;
        buttonTexts[index].color = selectedText;

        descriptionText.text = descriptions[index];

        PlaySound(navigateSound);
    }

    void SetAllNormal()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            var colors = menuButtons[i].colors;
            colors.normalColor = normalBG;
            menuButtons[i].colors = colors;
            buttonTexts[i].color = normalText;
        }
    }

    void ConfirmSelection()
    {
        PlaySound(confirmSound);
        switch (currentIndex)
        {
            case 0:
                if (LoadingManager.Instance != null)
                    LoadingManager.Instance.LoadScene("SCN_Play");
                else
                    SceneManager.LoadScene("SCN_Play"); // Fallback directo
                break;

            case 1: Debug.Log("Settings - sin implementar"); break;
            case 2: ExitGame(); break;
        }
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}