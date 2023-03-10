using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public Text scoreTxt;
    public Text levelTxt;
    public Text livesTxt;
    public Text highscoreTxt;
    public Text goScoreTxt;

    public GameObject GameOverScreen;
    public InputField gameOverInput;
    public GameObject submitBtn;

    public HighScores hs;

    int localScore;

    public int highscore;

    private void Start() {
    }

    public void UpdateUI(int score, int level, int lives) {
        highscore = hs.currentHighscore;

        localScore = score;
        scoreTxt.text = "Score - " + score;
        levelTxt.text = "Level - " + level;
        livesTxt.text = "Lives - " + lives;

        if (score > highscore)
            highscoreTxt.text = "High Score - " + score;
        else
            highscoreTxt.text = "High Score - " + highscore;

    }

    public void GameOver() {
        goScoreTxt.text = localScore.ToString();
        GameOverScreen.SetActive (true);
    }

    public void SubmitScore() {
        string name = gameOverInput.text;
        if (!name.Contains (" ")) {
            string txt = name + " " +localScore + "\n";
            hs.AddNewScore (txt);
            gameOverInput.gameObject.SetActive (false);
            submitBtn.SetActive (false);
        }
    }

    public void PlayAgain() {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
    }

    public void ExitGame() {
        SceneManager.LoadScene (0);
    }
}
