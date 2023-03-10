using UnityEngine;

public class Walls{
    public GameObject Wall;
    public Vector3 Pos;

    public Vector3 GetPos() {
        return this.Pos;
    }
    
    public Walls(GameObject wall, Vector3 pos) {
        this.Wall = wall;
        this.Pos = pos;
    }
}
