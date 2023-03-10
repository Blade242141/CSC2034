using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scores : IComparable<scores> {
    public string Name;
    public int Score;

    public string GetName() {
        return Name;
    }

    public int GetScore() {
        return Score;
    }

    public scores(string name, int score) {
        this.Name = name;
        this.Score = score;
    }

    public int CompareTo(scores s) {        // A null value means that this object is greater.
        if (s == null) {
            return 1;
        } else {
            return this.Score.CompareTo (s.Score);
        }
    }
}
