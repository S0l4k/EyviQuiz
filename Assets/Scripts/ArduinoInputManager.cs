using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ArduinoInputManager : MonoBehaviour
{
    [Header("Arduino Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;

    [Header("UI Host Controls")]
    public GameObject correctAnswerButton;
    public GameObject wrongAnswerButton;

    [Header("Host Next Buttons")]
    
    public GameObject nextRoundButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] playerBuzzSounds; 
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;

    [Header("Player Highlights")]
    public GameObject[] playerHighlights;
    public Canvas uiCanva_P; 
    public Camera mainCamera_p;
    public RectTransform[] playerUIElements_p;
    [Header("Player Highlight Anchors")]
    public Transform[] playerHighlightAnchors;

    [Header("Host Highlights")]
    public GameObject[] hostHighlights;
    public Canvas uiCanva_l;
    public Camera mainCamera_l;
    public RectTransform[] playerUIElements_l;
    [Header("Player Highlight Anchors")]
    public Transform[] leaderHighlightAnchors;

    private SerialPort serialPort;
    private bool playerBuzzed = false;
    public bool canBuzz = false;
    private int currentPlayer = -1;

    public QuizSetup quizSetup;

    void Start()
    {
        correctAnswerButton.SetActive(false);
        wrongAnswerButton.SetActive(false);

        serialPort = new SerialPort(portName, baudRate);
        try
        {
            serialPort.Open();
            serialPort.ReadTimeout = 50;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Błąd otwierania portu: " + e.Message);
        }
        
      
    }

    void Update()
    {
        if (!playerBuzzed && canBuzz)
        {
            // --- klawiatura do testów ---
            if (Input.GetKeyDown(KeyCode.Alpha1)) SimulateBuzz(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SimulateBuzz(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SimulateBuzz(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SimulateBuzz(4);

            // --- Arduino ---
            while (serialPort.BytesToRead > 0)
            {
                try
                {
                    string data = serialPort.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        int playerNumber;
                        if (int.TryParse(data, out playerNumber))
                        {
                            PlayerBuzz(playerNumber);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Błąd odczytu: " + e.Message);
                }
            }
        }
    }

    public void OnCorrectAnswer()
    {
        Debug.Log("Prowadzący: dobra odpowiedź gracza " + (currentPlayer + 1));
        if (currentPlayer >= 0 && quizSetup != null)
        {
            quizSetup.AddPoint(currentPlayer);
        }

       
        PlaySound(correctAnswerSound);
        SendToArduino("0");
        ResetBuzz();
    }

    public void OnWrongAnswer()
    {
        Debug.Log("Prowadzący: zła odpowiedź gracza " + (currentPlayer + 1));

        
        PlaySound(wrongAnswerSound);
        SendToArduino("0");
        ResetBuzz();
    }

    private void ResetBuzz()
    {
        correctAnswerButton.SetActive(false);
        wrongAnswerButton.SetActive(false);
        playerBuzzed = false;
        currentPlayer = -1;
        ClearHighlights();
        OnNextQuestion();
        nextRoundButton.SetActive(true);
    }


    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    void PlayerBuzz(int playerNumber)
    {
        int playerIndex = playerNumber - 1;
        if (playerIndex < 0 || playerIndex >= quizSetup.playerScores.Length) return;

        if (quizSetup.playerScores[playerIndex] < 0)
        {
            Debug.Log("Gracz " + playerNumber + " jest wyeliminowany i nie może się zgłaszać!");
            return;
        }

        currentPlayer = playerIndex;
        Debug.Log("Gracz zgłosił się: " + playerNumber);

       
        if (playerIndex < playerBuzzSounds.Length)
            PlaySound(playerBuzzSounds[playerIndex]);

        correctAnswerButton.SetActive(true);
        wrongAnswerButton.SetActive(true);
        playerBuzzed = true;
        canBuzz = false;

        HighlightPlayer(playerIndex);
        nextRoundButton.SetActive(false);
    }

    private void HighlightPlayer(int playerIndex)
    {
        ClearHighlights();

        if (playerIndex >= 0
            && playerIndex < playerHighlights.Length
            && playerHighlights[playerIndex] != null
            && playerIndex < playerUIElements_l.Length
            && playerUIElements_l [playerIndex] != null)
        {
           
            Vector3 screenPos_l = RectTransformUtility.WorldToScreenPoint(uiCanva_l.worldCamera, playerUIElements_l[playerIndex].position);
            Vector3 screenPos_p = RectTransformUtility.WorldToScreenPoint(uiCanva_P.worldCamera, playerUIElements_p[playerIndex].position);
           
            Vector3 worldPos_l = mainCamera_l.ScreenToWorldPoint(new Vector3(screenPos_l.x, screenPos_l.y, 16f));
            Vector3 worldPos_p = mainCamera_p.ScreenToWorldPoint(new Vector3(screenPos_p.x, screenPos_p.y, 16f));

            
            hostHighlights[playerIndex].transform.position = worldPos_l;
            hostHighlights[playerIndex].SetActive(true);
            playerHighlights[playerIndex].transform.position=worldPos_p;
            playerHighlights[playerIndex].SetActive(true) ;
        }
    }

    private void ClearHighlights()
    {
        foreach (var img in playerHighlights)
        {
            if (img != null)
            {
                img.SetActive(false);
            }
        }
        foreach (var img in hostHighlights)
        {
            if (img != null)
            {
                img.SetActive(false);
            }
        }
    }

    void SimulateBuzz(int playerNumber)
    {
        PlayerBuzz(playerNumber);
    }

    public void OnNextQuestion()
    {
        Debug.Log("Prowadzący: następne pytanie – gracze mogą się zgłaszać");
        canBuzz = true;
        playerBuzzed = false;
        
        nextRoundButton.SetActive(false);
    }

    public void OnNextRound()
    {
        Debug.Log("Prowadzący: następna runda");
        int eliminatedPlayer = quizSetup.NextRound();

        if (eliminatedPlayer >= 0)
        {
            
            SendToArduino("-" + (eliminatedPlayer + 1));
        }

        canBuzz = false;
        
        nextRoundButton.SetActive(false);
    }
    public void EnableBuzzing()
    {
        Debug.Log("Intro zakończone – gracze mogą się zgłaszać");
        canBuzz = true;
        playerBuzzed = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void SendToArduino(string message)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.WriteLine(message);
                Debug.Log("Wysłano do Arduino: " + message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Błąd wysyłania do Arduino: " + e.Message);
            }
        }
    }
}

