using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class HighScores : MonoBehaviour {

    string LOC;
    public int currentHighscore = 0;

    List<scores> score = new List<scores>();

    public string topTen = "";

    // Start is called before the first frame update
    void Start() {
        LOC = Directory.GetParent (Application.dataPath).ToString () + "/scores.txt";

        GetCurrentHighScore ();
        GetTopScores ();
    }

    void GetCurrentHighScore() {
        if (File.Exists (LOC)){
            //File Exsists
            FileInfo highscoreFile = new FileInfo (LOC);
            StreamReader r = highscoreFile.OpenText ();
            string text;

            do {
                text = r.ReadLine ();
                if (text == null)
                    break;
                string [] split = text.Split (' ');
                text = split [1];

                int i = int.Parse (text);
                if (i > 0) {
                    score.Add (new scores (split [0], i));
                    if (i >= currentHighscore) {
                        currentHighscore = i;
                    }
                }
            } while (text != null);
            r.Close ();
        } else {
            //File does not exsist
            currentHighscore = 0;
            File.WriteAllText (LOC, "");
        }
       

    }

    public void GetTopScores() {
        score.Sort();
        int n = 0;

        topTen = "";

        if (score.Count < 10)
            n = score.Count;
        else
            n = 10;

        int c = 1;
        for (int i = n-1; i>=0; i--) {
            topTen += c + " - " + score [i].GetName() + " - " + score [i].GetScore() + "\n";
            c++;
        }
    }

    public void AddNewScore(string line) {
        FileStream fs = new FileStream (LOC, FileMode.Append, FileAccess.Write, FileShare.Write);
        fs.Close ();
        StreamWriter w = new StreamWriter (LOC, true, Encoding.ASCII);
        w.WriteLine ("\n" + line);
        w.Close ();
    }
}
