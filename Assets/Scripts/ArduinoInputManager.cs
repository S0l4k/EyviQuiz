using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;

public class ArduinoInputManager : MonoBehaviour
{
    [Header("Arduino Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;

    [Header("UI Host Controls")]
    public GameObject correctAnswerButton;
    public GameObject wrongAnswerButton;



    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] playerBuzzSounds;
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;

    [Header("UI Highlights (Glow under players)")]
    public GameObject[] playerGlowHost;
    public GameObject[] playerGlowPlayers;

    private SerialPort serialPort;
    private bool playerBuzzed = false;
    public bool canBuzz = false;
    private int currentPlayer = -1;

    public QuizSetup quizSetup;

    void Start()
    {
        correctAnswerButton.SetActive(false);
        wrongAnswerButton.SetActive(false);
    }

    void Update()
    {
        if (!playerBuzzed && canBuzz)
        {
            // --- TESTY z klawiatury ---
            if (Input.GetKeyDown(KeyCode.Alpha1)) SimulateBuzz(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SimulateBuzz(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SimulateBuzz(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SimulateBuzz(4);

            // --- Arduino ---
            if (serialPort != null && serialPort.IsOpen && serialPort.BytesToRead > 0)
            {
                try
                {
                    string data = serialPort.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (int.TryParse(data, out int playerNumber))
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
        Debug.Log("Poprawna odpowiedź gracza " + (currentPlayer + 1));
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
        Debug.Log("Zła odpowiedź gracza " + (currentPlayer + 1));
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

       
        if (quizSetup.playerScores[playerIndex] < 0 || !quizSetup.playerActive[playerIndex])
        {
            Debug.Log("Gracz " + playerNumber + " nie może się zgłaszać!");
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
        
    }


    private void HighlightPlayer(int playerIndex)
    {
        ClearHighlights();
        if (playerIndex >= 0)
        {
            if (playerIndex < playerGlowHost.Length && playerGlowHost[playerIndex] != null)
                playerGlowHost[playerIndex].SetActive(true);

            if (playerIndex < playerGlowPlayers.Length && playerGlowPlayers[playerIndex] != null)
                playerGlowPlayers[playerIndex].SetActive(true);
        }
    }

    private void ClearHighlights()
    {
        foreach (var glow in playerGlowHost)
            if (glow != null) glow.SetActive(false);

        foreach (var glow in playerGlowPlayers)
            if (glow != null) glow.SetActive(false);
    }

    void SimulateBuzz(int playerNumber)
    {
        PlayerBuzz(playerNumber);
    }

    public void OnNextQuestion()
    {
        Debug.Log("Następne pytanie – gracze mogą się zgłaszać");
        canBuzz = true;
        playerBuzzed = false;
       
    }

    public void OnNextRound()
    {
        Debug.Log("Rozpoczęcie finału");
        canBuzz = false;
      
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

    public void UseExternalPort(SerialPort port, string name)
    {
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();

        serialPort = port;
        portName = name;

        if (!serialPort.IsOpen)
        {
            try
            {
                serialPort.Open();
                serialPort.ReadTimeout = 50;
                Debug.Log("ArduinoInputManager: używa portu z PortSelector -> " + portName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("ArduinoInputManager: błąd otwierania portu: " + e.Message);
            }
        }
    }
}
