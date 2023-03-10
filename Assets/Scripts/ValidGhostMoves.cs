using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidGhostMoves {
    public bool isValid;
    public Vector3 pos;

    public bool GetIsValid() {
        return this.isValid;
    }

    public Vector3 GetPos() {
        return this.pos;
    }

    public void SetValid(bool valid) {
        this.isValid = valid;
    }

    public ValidGhostMoves(bool isValid, Vector3 pos) {
        this.isValid = isValid;
        this.pos = pos;
    }
}
