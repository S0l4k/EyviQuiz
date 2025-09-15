using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuizSetup : MonoBehaviour
{
    private int currentRound = 1;

    [Header("UI Elements")]
    public Button startQuizButton;
    public GameObject playerNamesPanel;
    public TMP_InputField[] playerNameInputs;
    public Button nextButton;
    public GameObject ArduinoConnection;

    [Header("Start Backgrounds")]
    public GameObject playersBackground;
    public GameObject leaderBackground;
    public GameObject keyboard;

    [Header("Round Intros")]
    public GameObject container_p;
    public GameObject container_l;
    public TMP_Text roundText_p;
    public TMP_Text roundText_l;
    public Animator conteiner_pAnimator;
    public Animator conteiner_lAnimator;

    [Header("Game Backgrounds")]
    public GameObject playersGameBackground;
    public GameObject leaderGameBackground;
    public GameObject leaderGameAnim;
    public GameObject playersGameAnim;

    [Header("Game Animators")]
    public Animator playersGameAnimator;
    public Animator leaderGameAnimator;

    [Header("Player Name Displays")]
    public TMP_Text[] playerNameTextsHost;
    public TMP_Text[] playerNameTextsPlayers;

    [Header("Scores")]
    public TMP_Text[] playerScoreTextsHost;
    public TMP_Text[] playerScoreTextsPlayers;

    [Header("Ending")]
    public TMP_Text finalWinnerText;
    public TMP_Text finalWinnerTextHost;
    public GameObject playersFinalScreen;
    public GameObject leaderFinalScreen;
    public GameObject leaderAnimFinal;
    public GameObject playerAnimFinal;
    public GameObject playerFinalText;
    public GameObject playerFinalTextHost;
    public AudioClip winningMusic;

    [Header("Final Button")]
    public Button endFinalButton;

    private string[] playerNames = new string[4];
    public int[] playerScores = new int[4];
    private List<int> finalists = new List<int>();
    public bool[] playerActive = new bool[4];

    [Header("References")]
    public ArduinoInputManager arduinoInputManager;

    void Start()
    {
        playerNamesPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        startQuizButton.onClick.AddListener(OnStartQuiz);
        nextButton.onClick.AddListener(OnNext);

        foreach (var input in playerNameInputs)
            input.onValueChanged.AddListener(delegate { CheckPlayerNames(); });

        for (int i = 0; i < playerScores.Length; i++)
            playerScores[i] = 0;

        for (int i = 0; i < playerActive.Length; i++)
            playerActive[i] = true;

        if (endFinalButton != null)
        {
            endFinalButton.gameObject.SetActive(false);
            endFinalButton.onClick.AddListener(EndGame);
        }

        UpdateScoreUI();

        playersBackground.SetActive(true);
        leaderBackground.SetActive(true);
        ArduinoConnection.SetActive(true);
        container_p.SetActive(false);
        container_l.SetActive(false);
        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);
        keyboard.SetActive(false);
        playersGameAnim.SetActive(false);
        leaderGameAnim.SetActive(false);
        playersFinalScreen.SetActive(false);
        leaderFinalScreen.SetActive(false);
    }

    void OnStartQuiz()
    {
        startQuizButton.gameObject.SetActive(false);
        playerNamesPanel.SetActive(true);
        ArduinoConnection.SetActive(false);
    }

    void CheckPlayerNames()
    {
        bool allFilled = true;
        for (int i = 0; i < playerNameInputs.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(playerNameInputs[i].text))
            {
                allFilled = false;
                break;
            }
        }
        nextButton.gameObject.SetActive(allFilled);
    }

    void OnNext()
    {
        string roundLabel = "Manche 1";
        for (int i = 0; i < playerNameInputs.Length; i++)
            playerNames[i] = playerNameInputs[i].text.Trim();

        playerNamesPanel.SetActive(false);
        playersBackground.SetActive(false);
        leaderBackground.SetActive(false);
        keyboard.SetActive(false);

        for (int i = 0; i < playerNameTextsHost.Length; i++)
            playerNameTextsHost[i].text = playerNames[i];

        for (int i = 0; i < playerNameTextsPlayers.Length; i++)
            playerNameTextsPlayers[i].text = playerNames[i];

        ShowRoundIntro(roundLabel);
    }

    private void ShowRoundIntro(string roundLabel)
    {
        roundText_p.text = roundLabel;
        roundText_l.text = roundLabel;
        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);
        container_p.SetActive(true);
        container_l.SetActive(true);
        StartCoroutine(RoundIntroCoroutine());
    }

    private IEnumerator RoundIntroCoroutine()
    {
        yield return new WaitForSeconds(4f);
        container_p.SetActive(false);
        container_l.SetActive(false);
        playersGameBackground.SetActive(true);
        leaderGameBackground.SetActive(true);

        if (arduinoInputManager != null) arduinoInputManager.EnableBuzzing();
        if (playersGameAnimator != null) playersGameAnimator.SetTrigger("Enter");
        if (leaderGameAnimator != null) leaderGameAnimator.SetTrigger("Enter");

        yield return new WaitForSeconds(1f);
        playersGameAnim.SetActive(true);
        leaderGameAnim.SetActive(true);
    }

    public void AddPoint(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerScores.Length)
            return;

        
        if (currentRound == 1 && !playerActive[playerIndex])
            return;

        playerScores[playerIndex]++;
        UpdateScoreUI();

        if (currentRound == 1 && playerScores[playerIndex] >= 5)
        {
            
            if (!finalists.Contains(playerIndex))
                finalists.Add(playerIndex);

            playerActive[playerIndex] = false;
            Debug.Log("Gracz " + playerNames[playerIndex] + " zdobył 5 punktów – jego przycisk zablokowany");

            
            for (int i = 0; i < playerScores.Length; i++)
            {
                if (i != playerIndex && playerScores[i] >= 0)
                    playerScores[i] = 0;
            }

            UpdateScoreUI();

            
            if (finalists.Count == 2)
            {
                Debug.Log("Rozpoczęcie finału");
                StartFinalRound();
            }
        }
    }

    private void StartFinalRound()
    {
        currentRound = 2;

        
        foreach (int idx in finalists)
            playerScores[idx] = 0;

       
        for (int i = 0; i < playerScores.Length; i++)
        {
            if (!finalists.Contains(i))
            {
                playerScores[i] = -1;
                playerNameTextsHost[i].gameObject.SetActive(false);
                playerNameTextsPlayers[i].gameObject.SetActive(false);
                playerScoreTextsHost[i].gameObject.SetActive(false);
                playerScoreTextsPlayers[i].gameObject.SetActive(false);
            }
        }

        UpdateScoreUI();
        ShowRoundIntro("Final");

        
        for (int i = 0; i < playerActive.Length; i++)
        {
            playerActive[i] = finalists.Contains(i);
        }

        
        if (endFinalButton != null)
            endFinalButton.gameObject.SetActive(true);
    }

    void UpdateScoreUI()
    {
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScoreTextsHost[i].text = playerScores[i] >= 0 ? playerScores[i].ToString() : "-";
            playerScoreTextsPlayers[i].text = playerScores[i] >= 0 ? playerScores[i].ToString() : "-";
        }
    }

    public void EndGame()
    {
        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);
        playersFinalScreen.SetActive(true);
        leaderFinalScreen.SetActive(true);
        leaderAnimFinal.SetActive(true);
        playerAnimFinal.SetActive(true);
        playerFinalText.SetActive(true);
        playerFinalTextHost.SetActive(true);

        int winnerIndex = -1;
        int highestScore = -1;
        foreach (int idx in finalists)
        {
            if (playerScores[idx] > highestScore)
            {
                highestScore = playerScores[idx];
                winnerIndex = idx;
            }
        }

        finalWinnerText.text = winnerIndex != -1 ? playerNames[winnerIndex] : "Brak zwycięzcy";
        finalWinnerTextHost.text = finalWinnerText.text;

        if (winningMusic != null && arduinoInputManager.audioSource != null)
            arduinoInputManager.audioSource.PlayOneShot(winningMusic);

        
        if (arduinoInputManager != null)
            arduinoInputManager.SendToArduino("EndGame");
    }

    public void ResetQuiz()
    {
        finalists.Clear();
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = 0;
            playerScoreTextsHost[i].gameObject.SetActive(true);
            playerScoreTextsPlayers[i].gameObject.SetActive(true);
            playerNameTextsHost[i].gameObject.SetActive(true);
            playerNameTextsPlayers[i].gameObject.SetActive(true);

            playerActive[i] = true;

            if (arduinoInputManager != null)
                arduinoInputManager.SendToArduino("9");
        }

        UpdateScoreUI();
        currentRound = 1;

        leaderAnimFinal.SetActive(false);
        playerAnimFinal.SetActive(false);
        playerFinalText.SetActive(false);
        playerFinalTextHost.SetActive(false);
        playersFinalScreen.SetActive(false);
        leaderFinalScreen.SetActive(false);
        playersBackground.SetActive(true);
        leaderBackground.SetActive(true);
        ArduinoConnection.SetActive(true);
        startQuizButton.gameObject.SetActive(true);

        if (endFinalButton != null)
            endFinalButton.gameObject.SetActive(false);
    }
}
