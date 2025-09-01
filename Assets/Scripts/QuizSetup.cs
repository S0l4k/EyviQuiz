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
    public GameObject keyboard;

    [Header("Round Intros (Players + Leader)")]
    public GameObject container_p;
    public GameObject container_l;
    public TMP_Text roundText_p;
    public TMP_Text roundText_l;
    private string[] roundNames = { "Manche 1", "Manche 2", "Finale" };
    public Animator conteiner_pAnimator;
    public Animator conteiner_lAnimator; 


    [Header("Game Backgrounds (After Intro)")]
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

    private string[] playerNames = new string[4];
    public int[] playerScores = new int[4];

    [Header("References")]
    public ArduinoInputManager arduinoInputManager;


    private bool isRoundActive = false;
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
            playerScores[i] = 0;

        UpdateScoreUI();

        
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
        keyboard.SetActive(true);
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
        for
            (int i = 0; i < playerNameInputs.Length; i++)
            playerNames[i] = playerNameInputs[i].text.Trim();
        playerNamesPanel.SetActive(false);
        playersBackground.SetActive(false);
        leaderBackground.SetActive(false);
        keyboard.SetActive(false);
        for
            (int i = 0; i < playerNameTextsHost.Length; i++)
            playerNameTextsHost[i].text = playerNames[i];
        for
            (int i = 0; i < playerNameTextsPlayers.Length; i++)
            playerNameTextsPlayers[i].text = playerNames[i];
        Debug.Log("Gracze: " + string.Join(", ", playerNames));
        ShowRoundIntro(roundLabel);
    }
    private void ShowRoundIntro(string roundLabel)
    {
        isRoundActive = true;

        
        roundText_p.text = roundLabel;
        roundText_l.text = roundLabel;

       
        playersGameBackground.SetActive(false);
        leaderGameBackground.SetActive(false);

        
        container_p.SetActive(false);
        container_l.SetActive(false);
        container_p.SetActive(true);
        container_l.SetActive(true);

        
        StartCoroutine(RoundIntroCoroutine());
    }
    private IEnumerator RoundIntroCoroutine()
    {
        yield return new WaitForSeconds(3f);

        
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

    public int NextRound()
    {
        currentRound++;
        leaderGameAnim.SetActive(false);
        playersGameAnim.SetActive(false);
        container_p.SetActive(false);
        container_l.SetActive(false);

        if (currentRound > totalRounds)
        {
            EndGame();
            return -1;
        }

        string roundLabel = "";
        switch (currentRound)
        {
            case 1: roundLabel = "Manche 1"; break;
            case 2: roundLabel = "Manche 2"; break;
            case 3: roundLabel = "Finale"; break;
            default: EndGame(); return -1;
        }

        StartCoroutine(HandleRoundTransition(roundLabel));
        return -1;
    }
    private void ShowRound(string roundLabel, bool withReset = false)
    {
        roundText_p.text = roundLabel;
        roundText_l.text = roundLabel;

        container_p.SetActive(false);
        container_l.SetActive(false);
        container_p.SetActive(true);
        container_l.SetActive(true);

        if (withReset) StartCoroutine(RoundIntroCoroutine());
    }

    IEnumerator HandleRoundTransition(string roundLabel)
    {
        
        if (playersGameAnimator != null) playersGameAnimator.SetTrigger("Exit");
        if (leaderGameAnimator != null) leaderGameAnimator.SetTrigger("Exit");

        yield return new WaitForSeconds(1f);

        
        int eliminatedPlayer = EliminateLowestScorePlayer();

        
        for (int i = 0; i < playerScores.Length; i++)
            if (playerScores[i] >= 0)
                playerScores[i] = 0;

        UpdateScoreUI();

        
        if (eliminatedPlayer >= 0 && arduinoInputManager != null)
            arduinoInputManager.SendToArduino("-" + (eliminatedPlayer + 1));

       
        ShowRoundIntro(roundLabel);
    }


    private int EliminateLowestScorePlayer()
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

        return playerToEliminate;
    }

    private void EndGame()
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
        for (int i = 0; i < playerScores.Length; i++)
        {
            if (playerScores[i] > highestScore)
            {
                highestScore = playerScores[i];
                winnerIndex = i;
            }
        }

        if (winnerIndex != -1)
            finalWinnerText.text = playerNames[winnerIndex];
        else
            finalWinnerText.text = "No winner!";

        
        if (arduinoInputManager != null)
        {
            int lastEliminated = -1;
            for (int i = 0; i < playerScores.Length; i++)
            {
                if (i != winnerIndex && playerScores[i] >= 0)
                    lastEliminated = i;
            }

            if (lastEliminated >= 0)
                arduinoInputManager.SendToArduino("-" + (lastEliminated + 1));
        }
    }

    void SavePlayerNames()
    {
        for (int i = 0; i < playerNameInputs.Length; i++)
        {
            playerNames[i] = playerNameInputs[i].text;
            playerNameTextsHost[i].text = playerNames[i];
            playerNameTextsPlayers[i].text = playerNames[i];
        }
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

        startQuizButton.gameObject.SetActive(true);
    }
}
