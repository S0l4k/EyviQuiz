using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuizSetup : MonoBehaviour
{
    private int currentRound = 1;
    private int totalRounds = 3; 

    [Header("UI Elements")]
    public Button startQuizButton;
    public GameObject playerNamesPanel;
    public TMP_InputField[] playerNameInputs;
    public Button nextButton;

    [Header("Start Backgrounds (Initial UI Panels)")]
    public GameObject playersBackground;
    public GameObject leaderBackground;

    [Header("Round Intros (Players + Leader)")]
    public GameObject playersRound1Background;
    public GameObject leaderRound1Background;
    public GameObject playersRound2Background;
    public GameObject leaderRound2Background;
    public GameObject playersRound3Background;
    public GameObject leaderRound3Background;

    [Header("Game Backgrounds (After Intro)")]
    public GameObject playersGameBackground;
    public GameObject leaderGameBackground;

    [Header("Player Name Displays")]
    public TMP_Text[] playerNameTextsHost;
    public TMP_Text[] playerNameTextsPlayers;

    [Header("Scores")]
    public TMP_Text[] playerScoreTextsHost;
    public TMP_Text[] playerScoreTextsPlayers;

    [Header("Ending")]
    public GameObject playersFinalScreen;
    public GameObject leaderFinalScreen;
    public TMP_Text finalWinnerText;

    private string[] playerNames = new string[4];
    public int[] playerScores = new int[4];

    [Header("References")]
    public ArduinoInputManager arduinoInputManager;

    void Start()
    {
        playerNamesPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        startQuizButton.onClick.AddListener(OnStartQuiz);
        nextButton.onClick.AddListener(OnNext);

        foreach (var input in playerNameInputs)
        {
            input.onValueChanged.AddListener(delegate { CheckPlayerNames(); });
        }
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = 0;
        }
        UpdateScoreUI();

        playersRound1Background.SetActive(false);
        leaderRound1Background.SetActive(false);
        playersRound2Background.SetActive(false);
        leaderRound2Background.SetActive(false);
        playersRound3Background.SetActive(false);
        leaderRound3Background.SetActive(false);
        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);
        playersFinalScreen.SetActive(false);
        leaderFinalScreen.SetActive(false);
    }

    void OnStartQuiz()
    {
        startQuizButton.gameObject.SetActive(false);
        playerNamesPanel.SetActive(true);
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
        for (int i = 0; i < playerNameInputs.Length; i++)
        {
            playerNames[i] = playerNameInputs[i].text.Trim();
        }

        playerNamesPanel.SetActive(false);
        playersBackground.SetActive(false);
        leaderBackground.SetActive(false);

        for (int i = 0; i < playerNameTextsHost.Length; i++)
            playerNameTextsHost[i].text = playerNames[i];

        for (int i = 0; i < playerNameTextsPlayers.Length; i++)
            playerNameTextsPlayers[i].text = playerNames[i];

        Debug.Log("Gracze: " + string.Join(", ", playerNames));

        StartCoroutine(ShowRoundIntro(playersRound1Background, leaderRound1Background, 3f, playersGameBackground, leaderGameBackground));
    }

    IEnumerator ShowRoundIntro(GameObject playersIntro, GameObject leaderIntro, float delay, GameObject playersFinal, GameObject leaderFinal)
    {
        playersIntro.SetActive(true);
        leaderIntro.SetActive(true);

        yield return new WaitForSeconds(delay);

        playersIntro.SetActive(false);
        leaderIntro.SetActive(false);

        playersFinal.SetActive(true);
        leaderFinal.SetActive(true);

        if (arduinoInputManager != null)
        {
            arduinoInputManager.EnableNextButtons();
        }
    }

    public void AddPoint(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerScores.Length)
        {
            playerScores[playerIndex]++;
            UpdateScoreUI();
        }
    }

    void UpdateScoreUI()
    {
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScoreTextsHost[i].text = playerScores[i].ToString();
            playerScoreTextsPlayers[i].text = playerScores[i].ToString();
        }
    }

    public void NextRound()
    {
       
        EliminateLowestScorePlayer();

        
        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] >= 0)
                playerScores[i] = 0;
        }
        UpdateScoreUI();

        currentRound++;

        playersRound1Background.SetActive(false);
        leaderRound1Background.SetActive(false);
        playersRound2Background.SetActive(false);
        leaderRound2Background.SetActive(false);
        playersRound3Background.SetActive(false);
        leaderRound3Background.SetActive(false);

        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);

        if (currentRound > totalRounds)
        {
            EndGame();
        }
        else
        {
            switch (currentRound)
            {
                case 2:
                    StartCoroutine(ShowRoundIntro(playersRound2Background, leaderRound2Background, 4f, playersGameBackground, leaderGameBackground));
                    break;
                case 3:
                    StartCoroutine(ShowRoundIntro(playersRound3Background, leaderRound3Background, 4f, playersGameBackground, leaderGameBackground));
                    break;
            }
        }
    }

    private void EliminateLowestScorePlayer()
    {
        int minScore = int.MaxValue;
        int playerToEliminate = -1;

        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] >= 0 && playerScores[i] < minScore)
            {
                minScore = playerScores[i];
                playerToEliminate = i;
            }
        }

        if (playerToEliminate != -1)
        {
            Debug.Log("Eliminowany gracz: " + playerNames[playerToEliminate]);
            playerScores[playerToEliminate] = -1;

            playerScoreTextsHost[playerToEliminate].gameObject.SetActive(false);
            playerScoreTextsPlayers[playerToEliminate].gameObject.SetActive(false);
            playerNameTextsHost[playerToEliminate].gameObject.SetActive(false);
            playerNameTextsPlayers[playerToEliminate].gameObject.SetActive(false);
        }
    }

    private void EndGame()
    {
        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);

        playersFinalScreen.SetActive(true);
        leaderFinalScreen.SetActive(true);

        int winnerIndex = -1;
        int highestScore = -1;
        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] > highestScore)
            {
                highestScore = playerScores[i];
                winnerIndex = i;
            }
        }

        if (winnerIndex != -1)
            finalWinnerText.text = playerNames[winnerIndex] + " - Congratulations!";
        else
            finalWinnerText.text = "No winner!";
    }

    public void ResetQuiz()
    {
        
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = 0;

            playerScoreTextsHost[i].gameObject.SetActive(true);
            playerScoreTextsPlayers[i].gameObject.SetActive(true);
            playerNameTextsHost[i].gameObject.SetActive(true);
            playerNameTextsPlayers[i].gameObject.SetActive(true);
        }

        UpdateScoreUI();
        currentRound = 1;

        playersFinalScreen.SetActive(false);
        leaderFinalScreen.SetActive(false);

        playersBackground.SetActive(true);
        leaderBackground.SetActive(true);

        startQuizButton.gameObject.SetActive(true);
    }
}
