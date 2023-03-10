using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    #region Vars

    [SerializeField]
    int score = 0;
    [SerializeField]
    int level = 1;
    [SerializeField]
    int lives = 3;

    public List<Walls> walls = new List<Walls> ();

    public UIManager ui;

    enum GhostMode {
        Scatter,
        Chase,
        Frightened
    }

    [SerializeField]
    GhostMode ghostMode = GhostMode.Scatter;

    public bool reversed = false;

    float releasePink = 2;

    public bool redActive = false;
    public bool pinkActive = false;
    public bool blueActive = false;
    public bool orangeActive = false;

    bool pinkAllowed = false;
    bool blueAllowed = false;
    bool orangeAllowed = false;

    public GameObject redObj;
    public GameObject pinkObj;
    public GameObject blueObj;
    public GameObject orangeObj;

    float scatter1 = 0f;
    float chase1 = 0f;
    float scatter2 = 0f;
    float chase2 = 0f;
    float scatterFinal = 0f;

    float scaredTimer = 0f;

    float doublePointsCounter = 0f;
    float randomPowerUpCounter = 0f;
    bool beenActive = false;

    public GameObject randomPowerUp;

    public AudioSource audioSource;
    public AudioClip levelComplete;
    public AudioClip playerDied;

    #endregion

    // Start is called before the first frame update
    void Start() {
        SetUpWallPos ();
        SetUpLevel ();

        redActive = true;

        if (PlayerPrefs.GetInt ("score") > 0) {
            level = PlayerPrefs.GetInt ("level");
            lives = PlayerPrefs.GetInt ("lives");
            score = PlayerPrefs.GetInt ("score");
        }

        ui.UpdateUI (score, level, lives);

    }

    // Update is called once per frame
    void Update() {
        CheckActive ();

        //wait 2s then release pink
        if (!pinkActive && !pinkAllowed) {
            if (releasePink > 0) {
                releasePink -= Time.deltaTime;
            } else {
                pinkAllowed = true;
                ReleaseGhost (pinkObj.GetComponent<GhostPathFinding> ());
            }
        }
        if (scaredTimer > 0) {

            scaredTimer -= Time.deltaTime;
        } else if (scatter1 > 0) {
            SetGlobalScatter ();
            scatter1 -= Time.deltaTime;
        } else if (chase1 > 0) {
            SetGlobalChase ();
            chase1 -= Time.deltaTime;
        } else if (scatter2 > 0) {
            SetGlobalScatter ();
            scatter2 -= Time.deltaTime;
        } else if (chase2 > 0) {
            SetGlobalChase ();
            chase2 -= Time.deltaTime;
        } else if (scatterFinal > 0) {
            scatterFinal -= Time.deltaTime;
        } else {
            SetGlobalChase ();
        }

        if (!redActive) {
            TimedRelease (redObj.GetComponent<GhostPathFinding> ());
        }
        if (!pinkActive && pinkAllowed) {
            TimedRelease (pinkObj.GetComponent<GhostPathFinding> ());
        }
        if (!blueActive && blueAllowed) {
            TimedRelease (blueObj.GetComponent<GhostPathFinding> ());
        }
        if (!orangeActive && orangeAllowed) {
            TimedRelease (orangeObj.GetComponent<GhostPathFinding> ());
        }

        if (doublePointsCounter > 0)
            doublePointsCounter -= Time.deltaTime;

        if (randomPowerUpCounter > 0)
            randomPowerUpCounter -= Time.deltaTime;
        else
            randomPowerUp.SetActive(false);
    }

    public void CheckActive() {
        redActive = HouseToActive (redObj.GetComponent<GhostPathFinding> ());
        pinkActive = HouseToActive (pinkObj.GetComponent<GhostPathFinding> ());
        blueActive = HouseToActive (blueObj.GetComponent<GhostPathFinding> ());
        orangeActive = HouseToActive (orangeObj.GetComponent<GhostPathFinding> ());
    }

    void TimedRelease(GhostPathFinding gpf) {
        if (gpf.releaseTimer == -1)
            gpf.releaseTimer = Random.Range (2, 15);
        else if (gpf.releaseTimer > 0) {
            gpf.releaseTimer -= Time.deltaTime;
        } else {
            gpf.releaseTimer = -1;
            ReleaseGhost (gpf);
        }
    }

    bool HouseToActive(GhostPathFinding gpf) {
        if (gpf.isInHouse)
            return false;
        else
            return true;
    }

    void ReleaseGhost(GhostPathFinding gpf) {
        gpf.Release ();
    }

    public void IncreaseScore() {
        if (doublePointsCounter > 0) {
            score = score +2;
        } else {
            score++;
        }
        ui.UpdateUI (score, level, lives);
    }

    public void SetGlobalScatter() {
        ghostMode = GhostMode.Scatter;
        GameObject [] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts [i].GetComponent<GhostPathFinding> ().SetScatter ();
        }
    }

    public void SetGlobalChase() {
        ghostMode = GhostMode.Chase;
        GameObject [] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts [i].GetComponent<GhostPathFinding> ().SetChase ();
        }
    }

    public void SetGlobalFrightened() {
        SetUpScared ();
        ghostMode = GhostMode.Frightened;
        GameObject [] ghosts = GameObject.FindGameObjectsWithTag ("Ghost");
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts [i].GetComponent<GhostPathFinding> ().SetFrightened ();
        }
    }

    void SetUpWallPos() {
        GameObject [] go = GameObject.FindGameObjectsWithTag ("Wall");
        for (int i = 0; i < go.Length; i++) {
            Vector3 v = new Vector3 (go [i].transform.position.x, go [i].transform.position.y, go [i].transform.parent.position.z);
            Walls w = new Walls (go [i], v);
            walls.Add (w);
        }
    }

    public void PointCollected(int i, float f, int totalPellets) {
        ui.UpdateUI (score, level, lives);

        if (i >= 30 && !blueActive) {
            blueAllowed = true;
            ReleaseGhost (blueObj.GetComponent<GhostPathFinding>());
        }
        if (f <= i && !orangeActive) {
            orangeAllowed = true;
            ReleaseGhost (orangeObj.GetComponent<GhostPathFinding> ());
        }
        //When half the pellet are eaten, active powerup, only once per level
        if (i == (totalPellets / 2) || !beenActive) {
            randomPowerUp.SetActive (true);
            beenActive = true;
            randomPowerUpCounter = Random.Range (5, 20);
        }

    }

    public void ActivatePowerUp(GameObject go) {
        int i = Random.Range (0, 2);
        print (i);
        if (i == 0) {
            //Double Points
            doublePointsCounter = 5f;
            print ("DOUBLE");
        } else if (i == 1) {
            //Bounus Points
            score = score +50;
            print ("PLUS 50");
        } else if (i == 2) {
            print ("NUKE");
            //Kill Active ghosts
            if (redActive)
                redObj.GetComponent<GhostPathFinding> ().Died ();
            if (pinkActive)
                pinkObj.GetComponent<GhostPathFinding> ().Died ();
            if (blueActive)
                blueObj.GetComponent<GhostPathFinding> ().Died ();
            if (orangeActive)
                orangeObj.GetComponent<GhostPathFinding> ().Died ();
        }

        beenActive = true;
        go.SetActive (false);
    }

    void SetUpLevel() {
        if (level == 1) {
            scatter1 = 7f;
            chase1 = 20f;
            scatter2 = 5f;
            chase2 = 20f;
            scatterFinal = 5f;
            scaredTimer = 6f;
        } else if (level >= 2 && level < 5) {
            scatter1 = 7f;
            chase1 = 20f;
            scatter2 = 5f;
            chase2 = 1033f;
            scatterFinal = 1 / 60;
            scaredTimer = 5f;
        } else if (level >= 5) {
            scatter1 = 5f;
            chase1 = 0f;
            scatter2 = 5f;
            chase2 = 1037f;
            scatterFinal = 1/60f;
            scaredTimer = 2f;
        }

        if (level == 18)
            scaredTimer = 1f;
        else if (level == 19)
            scaredTimer = 0.5f;
        else if (level > 19)
            scaredTimer = 0.01f;
    }

    void SetUpScared() {
        if (level == 1)
            scaredTimer = 6f;
        else if (level >= 2 && level < 5)
            scaredTimer = 5f;
        else if (level >= 5)
            scaredTimer = 2f;
        else if (level == 18)
            scaredTimer = 1f;
        else if (level == 19)
            scaredTimer = 0.5f;
        else if (level > 19)
            scaredTimer = 0.01f;
    }

    public void PlayerDied() {

        audioSource.clip = playerDied;
        audioSource.Play ();

        if (lives <= 0) {
            PlayerPrefs.SetInt ("level", 1);
            PlayerPrefs.SetInt ("lives", 3);
            PlayerPrefs.SetInt ("score", 0);

            redObj.SetActive (false);
            pinkObj.SetActive (false);
            blueObj.SetActive (false);
            orangeObj.SetActive (false);

            GameObject.FindGameObjectWithTag ("Player").SetActive (false);

            ui.GameOver ();

            //Gameover
        } else {
            lives--;
            ui.UpdateUI (score, level, lives);
            ResetLevel ();
        }
    }

    public void ResetLevel() {
        //Save Data and reload scene
        redObj.GetComponent<GhostPathFinding> ().ResetMap ();
        pinkObj.GetComponent<GhostPathFinding> ().ResetMap ();
        blueObj.GetComponent<GhostPathFinding> ().ResetMap ();
        orangeObj.GetComponent<GhostPathFinding> ().ResetMap ();

        GameObject.FindGameObjectWithTag ("Player").GetComponent<Fellow>().ResetMap();
    }

    public void NewLevel() {
        if (!audioSource.isPlaying) {
            audioSource.clip = levelComplete;
            audioSource.Play ();
            StartCoroutine(WaitForAudio ());

            level++;
            PlayerPrefs.SetInt ("level", level);
            PlayerPrefs.SetInt ("lives", lives);
            PlayerPrefs.SetInt ("score", score);
        }
    }

    IEnumerator WaitForAudio() {
        redObj.SetActive(false);
        pinkObj.SetActive (false);
        blueObj.SetActive (false);
        orangeObj.SetActive (false);

        yield return new WaitForSeconds (audioSource.clip.length);
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
    }
}
