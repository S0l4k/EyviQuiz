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

    [Header("Host Next Buttons")]
    public GameObject nextQuestionButton;
    public GameObject nextRoundButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] playerBuzzSounds; 
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;

    [Header("Player Highlights")]
    public Image[] playerHighlights;

    [Header("Host Highlights")]
    public Image[] hostHighlights;

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
        
        foreach (var img in playerHighlights)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
        }
        foreach (var img in hostHighlights)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
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
            if (serialPort != null && serialPort.IsOpen)
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
                catch (System.TimeoutException) { }
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

        ResetBuzz();
    }

    public void OnWrongAnswer()
    {
        Debug.Log("Prowadzący: zła odpowiedź gracza " + (currentPlayer + 1));

        
        PlaySound(wrongAnswerSound);

        ResetBuzz();
    }

    private void ResetBuzz()
    {
        correctAnswerButton.SetActive(false);
        wrongAnswerButton.SetActive(false);
        playerBuzzed = false;
        currentPlayer = -1;
        EnableNextButtons();
        ClearHighlights();
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
    }

    private void HighlightPlayer(int index)
    {
        ClearHighlights(); 
        if (index >= 0 && index < playerHighlights.Length && playerHighlights[index] != null)
        {
            Color c = playerHighlights[index].color;
            c.a = 1f;
            playerHighlights[index].color = c;
        }

        if (index >= 0 && index < hostHighlights.Length && hostHighlights[index] != null)
        {
            Color c = hostHighlights[index].color;
            c.a = 1f;
            hostHighlights[index].color = c;
        }
    }

    private void ClearHighlights()
    {
        foreach (var img in playerHighlights)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
        }
        foreach (var img in hostHighlights)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
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
        nextQuestionButton.SetActive(false);
        nextRoundButton.SetActive(false);
    }

    public void OnNextRound()
    {
        Debug.Log("Prowadzący: następna runda");
        quizSetup.NextRound();
        canBuzz = false;
        nextQuestionButton.SetActive(false);
        nextRoundButton.SetActive(false);
    }

    public void EnableNextButtons()
    {
        nextQuestionButton.SetActive(true);
        nextRoundButton.SetActive(true);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

