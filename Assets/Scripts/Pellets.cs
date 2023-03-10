using UnityEngine;

public class Pellets {
    public GameObject Pellet;
    public bool Collected;

    public void SetCollected (bool c) {
        this.Collected = c;
    }

    public bool GetCollected() {
        return this.Collected;
    }

    public GameObject GetPelletObj() {
        return this.Pellet;
    }

    public Pellets(GameObject pellet, bool collected) {
        this.Pellet = pellet;
        this.Collected = collected;
    }

    public bool isSamePellet(GameObject p) {
        if (GameObject.ReferenceEquals (this.Pellet, p))
            return true;
        else
            return false;
    }
}
