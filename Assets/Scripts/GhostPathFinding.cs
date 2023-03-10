using System.Collections.Generic;
using UnityEngine;

public class GhostPathFinding : MonoBehaviour {
    enum GhostMode {
        Scatter,
        Chase,
        Frightened,
        Dead
    }

    enum GhostType {
        Red,
        Pink,
        Blue,
        Orange
    }

    public Transform ghostTransform;
    public Rigidbody ghostRigidbody;

    [SerializeField]
    public bool isInHouse = true;

    public Vector3 ghostHousePos;

    [SerializeField]
    Vector2 direction = Vector2.up;

    [SerializeField]
    float speed = 1.0f;

    [SerializeField]
    GhostMode ghostMode = GhostMode.Scatter;

    [SerializeField]
    GhostType ghostType;

    public GameManager gm;

    public Transform LoopCornerWall;

    readonly string N = "North";
    readonly string E = "East";
    readonly string S = "South";
    readonly string W = "West";

    public Material red;
    public Material pink;
    public Material blue;
    public Material orange;
    public Material scared;
    public Material deadGhost;

    public float releaseTimer = -1;

    // Start is called before the first frame update
    void Start() {
        AssignColours ();

        if (!gm.reversed)
            GoLeft (ghostTransform.position);
        else
            GoRight (ghostTransform.position);

        Physics.IgnoreLayerCollision (8, 9, true);
    }

    // Update is called once per frame
    void Update() {
        if (!isInHouse)
            JustKeepMoving ();
    }

    void AssignColours() {
        Material mat = null;
        if (ghostType == GhostType.Red)
            mat = red;
        else if (ghostType == GhostType.Pink)
            mat = pink;
        else if (ghostType == GhostType.Blue)
            mat = blue;
        else if (ghostType == GhostType.Orange)
            mat = orange;

        this.GetComponent<MeshRenderer> ().material = mat;
    }

    #region MoveGhosts

    void UpdateMoveRed(GameObject other) {
        //Scatter and Chase are the same for Red
        if (ghostMode == GhostMode.Scatter || ghostMode == GhostMode.Chase) {
            //Target Player
            CompleteTurn (ShortestDistanceTo (GameObject.FindGameObjectWithTag ("Player").transform.position, other), other);
        } else if (ghostMode == GhostMode.Frightened) {
            //Pseudorandom turns
            FrightenedMove (other);
        } else if (ghostMode == GhostMode.Dead) {
            CompleteTurn (ShortestDistanceTo (GameObject.FindGameObjectWithTag ("GhostHouse").transform.position, other), other);
        }
    }

    void UpdateMovePink(GameObject other) {
        //Targets 4 ints ahead of player
        if (ghostMode == GhostMode.Chase) {
            GameObject player = GameObject.FindGameObjectWithTag ("Player");

            Vector3 v = Vector3.zero;

            if (player.GetComponent<Fellow> ().direction == Vector2.left)
                v = new Vector3 (player.transform.position.x - 4, player.transform.position.y, player.transform.position.z);
            else if (player.GetComponent<Fellow> ().direction == Vector2.right)
                v = new Vector3 (player.transform.position.x + 4, player.transform.position.y, player.transform.position.z);
            else if (player.GetComponent<Fellow> ().direction == Vector2.up)
                v = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z + 4);
            else if (player.GetComponent<Fellow> ().direction == Vector2.down)
                v = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z - 4);

            CompleteTurn (ShortestDistanceTo (v, other), other);

        } else if (ghostMode == GhostMode.Scatter) {
            CompleteTurn (ShortestDistanceTo (LoopCornerWall.position, other), other);
        } else if (ghostMode == GhostMode.Frightened) {
            //Pseudorandom turns
            FrightenedMove (other);
        } else if (ghostMode == GhostMode.Dead) {
            CompleteTurn (ShortestDistanceTo (GameObject.FindGameObjectWithTag ("GhostHouse").transform.position, other), other);
        }
    }

    void UpdateMoveBlue(GameObject other) {
        //Get 2 tiles infront of player
        //Get Vector of Red to that tile
        //Times Vector by 2
        //Go to that tile
        if (ghostMode == GhostMode.Chase) {
            GameObject player = GameObject.FindGameObjectWithTag ("Player");

            Vector3 v = Vector3.zero;
            //Get position 2 tiles away from player
            if (player.GetComponent<Fellow> ().direction == Vector2.left)
                v = new Vector3 (player.transform.position.x - 2, player.transform.position.y, player.transform.position.z);
            else if (player.GetComponent<Fellow> ().direction == Vector2.right)
                v = new Vector3 (player.transform.position.x + 2, player.transform.position.y, player.transform.position.z);
            else if (player.GetComponent<Fellow> ().direction == Vector2.up)
                v = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z + 2);
            else if (player.GetComponent<Fellow> ().direction == Vector2.down)
                v = new Vector3 (player.transform.position.x, player.transform.position.y, player.transform.position.z - 2);

            float A = Vector3.Angle (v, ghostTransform.position);
            float c = (Vector3.Distance (v, ghostTransform.position)) * 2;

            //Work out X and Z value
            float x = c * Mathf.Sin (A);
            float z = c * Mathf.Cos (A);

            Vector3 point = new Vector3 (x, other.transform.position.y, z);

            CompleteTurn (ShortestDistanceTo (point, other), other);
        } else if (ghostMode == GhostMode.Scatter) {
            CompleteTurn (ShortestDistanceTo (LoopCornerWall.position, other), other);
        } else if (ghostMode == GhostMode.Frightened) {
            //Pseudorandom turns
            FrightenedMove (other);
        } else if (ghostMode == GhostMode.Dead) {
            CompleteTurn (ShortestDistanceTo (GameObject.FindGameObjectWithTag ("GhostHouse").transform.position, other), other);
        }
    }

    void UpdateMoveOrange(GameObject other) {
        //If player is <8 away, target is same as Red
        //If player is >8 away, Use Scatter Mode Target
        if (ghostMode == GhostMode.Chase) {
            if (Vector3.Distance (ghostTransform.position, GameObject.FindGameObjectWithTag ("Player").transform.position) <= 8) {
                //Target Loop
                CompleteTurn (ShortestDistanceTo (LoopCornerWall.position, other), other);
            } else {
                //Target Player Same As Red
                UpdateMoveRed (other);
            }
        } else if (ghostMode == GhostMode.Scatter) {
            CompleteTurn (ShortestDistanceTo (LoopCornerWall.position, other), other);
        } else if (ghostMode == GhostMode.Frightened) {
            //Pseudorandom turns
            FrightenedMove (other);
        } else if (ghostMode == GhostMode.Dead) {
            CompleteTurn (ShortestDistanceTo (GameObject.FindGameObjectWithTag ("GhostHouse").transform.position, other), other);
        }
    }

    string ShortestDistanceTo(Vector3 point, GameObject other) {
        Dictionary<string, ValidGhostMoves> cvp = CheckValidPaths (other.transform.position);
        Dictionary<string, float> distance = new Dictionary<string, float> ();

        string shortestDirection = "";
        float shortestDistance = 0.0f;

        if (other.tag == "4way") {
            cvp ["North"].SetValid (false);
            if (direction == Vector2.right)
                return "East";
            else if (direction == Vector2.left)
                return "West";
        }

        foreach (KeyValuePair<string, ValidGhostMoves> d in cvp) {
            if (d.Value.GetIsValid ()) {
                distance.Add (d.Key, Vector3.Distance (d.Value.pos, point));
            }
        }

        foreach (KeyValuePair<string, float> move in distance) {
            if (move.Value <= shortestDistance || shortestDistance == 0.0f) {
                shortestDistance = move.Value;
                shortestDirection = move.Key;
            }
        }
        return shortestDirection;
    }

    void CompleteTurn(string shortestDirection, GameObject other) {
        if (shortestDirection == "North")
            GoUp (other.transform.position);
        else if (shortestDirection == "East")
            GoRight (other.transform.position);
        else if (shortestDirection == "South")
            GoDown (other.transform.position);
        else if (shortestDirection == "West")
            GoLeft (other.transform.position);
    }

    void JustKeepMoving() {
        ghostRigidbody.MovePosition (transform.position + transform.forward * speed * Time.deltaTime);
    }

    void FrightenedMove(GameObject other) {
        Dictionary<string, ValidGhostMoves> cvp = CheckValidPaths (other.transform.position);
        Dictionary<string, ValidGhostMoves> validMoves = new Dictionary<string, ValidGhostMoves> ();
        string [] s = new string[4];

        foreach (KeyValuePair<string, ValidGhostMoves> d in cvp) {
            if (d.Value.GetIsValid () && other.tag != "4way") {
                validMoves.Add (d.Key, d.Value);
                for (int i = 0; 0<s.Length; i++) {
                    if (s[i] == null) {
                        s [i] = d.Key;
                        break;
                    }
                }
            }
        }

        int r = Random.Range(0, validMoves.Count);

        if (s [r] == N)
            GoUp (other.transform.position);
        else if (s [r] == E)
            GoRight (other.transform.position);
        else if (s [r] == S)
            GoDown (other.transform.position);
        else if (s [r] == W)
            GoLeft (other.transform.position);
        
    }

    void GoUp(Vector3 pos) {
        direction = Vector2.up;
        Vector3 v = new Vector3 (Mathf.RoundToInt(pos.x), transform.position.y, transform.position.z);
        ghostTransform.SetPositionAndRotation (v, Quaternion.Euler (0f, 0f, 0f));
    }

    void GoDown(Vector3 pos) {
        direction = Vector2.down;
        Vector3 v = new Vector3 (Mathf.RoundToInt (pos.x), transform.position.y, transform.position.z);
        ghostTransform.SetPositionAndRotation(v, Quaternion.Euler (0f, 180f, 0f));
    }

    void GoLeft(Vector3 pos) {
        direction = Vector2.left;
        Vector3 v = new Vector3 (transform.position.x, transform.position.y, Mathf.RoundToInt (pos.z));
        ghostTransform.SetPositionAndRotation (v, Quaternion.Euler (0f, -90f, 0f));
    }

    void GoRight(Vector3 pos) {
        direction = Vector2.right;
        Vector3 v = new Vector3 (transform.position.x, transform.position.y, Mathf.RoundToInt (pos.z));
        ghostTransform.SetPositionAndRotation (v, Quaternion.Euler (0f, 90f, 0f));
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "BendPellet" || other.tag == "PowerUp") {
            Dictionary<string, ValidGhostMoves> cvp = CheckValidPaths(other.transform.position);
            if (cvp ["North"].GetIsValid())
                GoUp (other.transform.position);
            else if (cvp ["East"].GetIsValid())
                GoRight (other.transform.position);
            else if (cvp ["South"].GetIsValid())
                GoDown (other.transform.position);
            else if (cvp ["West"].GetIsValid())
                GoLeft (other.transform.position);
        } else if (other.tag == "IntersPellet" || other.tag == "4way") {
            switch (ghostType) {
                case GhostType.Red:
                UpdateMoveRed (other.gameObject);
                break;
                case GhostType.Pink:
                UpdateMovePink (other.gameObject);
                break;
                case GhostType.Blue:
                UpdateMoveBlue (other.gameObject);
                break;
                case GhostType.Orange:
                UpdateMoveOrange (other.gameObject);
                break;
            }
        } else if (other.tag == "HousePellet" && ghostMode == GhostMode.Dead) {
            GoDown (other.transform.position);
        } else if (other.tag == "HousePellet" && ghostMode != GhostMode.Dead && direction == Vector2.up) {
            if (!gm.reversed) {
                GoLeft (other.transform.position);
            } else {
                GoRight (other.transform.position);
            }
        }

        if (other.tag == "HousePellet" && ghostMode == GhostMode.Dead) {
            ResetMap ();
        }

        if (other.tag == "Tele") {
            if (direction == Vector2.right)
                ghostTransform.SetPositionAndRotation (new Vector3 (0, ghostTransform.position.y, ghostTransform.position.z), ghostTransform.rotation);
            else if (direction == Vector2.left)
                ghostTransform.SetPositionAndRotation (new Vector3 (15, ghostTransform.position.y, ghostTransform.position.z), ghostTransform.rotation);
        }
    }

    Dictionary<string, ValidGhostMoves> CheckValidPaths(Vector3 currentPos) {
        List<Walls> w = gm.walls;

        Dictionary<string, ValidGhostMoves> cvp = new Dictionary<string, ValidGhostMoves> ();

        Vector3 North = new Vector3(currentPos.x, 0.5f, currentPos.z + 1);
        Vector3 South = new Vector3 (currentPos.x, 0.5f, currentPos.z - 1);
        Vector3 East = new Vector3 (currentPos.x + 1, 0.5f, currentPos.z);
        Vector3 West = new Vector3 (currentPos.x - 1, 0.5f, currentPos.z);

        Vector3 v = new Vector3 (5f, 0.5f, 0f);

        //Check direction coming from
        if (direction == Vector2.down)
            cvp.Add (N, new ValidGhostMoves (false, North));
        else
            cvp.Add (N, new ValidGhostMoves (true, North));

        if (direction == Vector2.left)
            cvp.Add (E, new ValidGhostMoves (false, East));
        else
            cvp.Add (E, new ValidGhostMoves (true, East));
           
        if (direction == Vector2.up)
            cvp.Add (S, new ValidGhostMoves (false, South));
        else
            cvp.Add (S, new ValidGhostMoves (true, South));
        if (direction == Vector2.right)
            cvp.Add (W, new ValidGhostMoves (false, West));
        else
            cvp.Add (W, new ValidGhostMoves (true, West));

        //Check if wall is blocking direction
        foreach (Walls wall in w) {
            if (wall.GetPos () == North)
                cvp [N] = new ValidGhostMoves(false, North);

            if (wall.GetPos () == East)
                cvp [E] = new ValidGhostMoves (false, East);

            if (wall.GetPos () == South)
                cvp [S] = new ValidGhostMoves (false, South);

            if (wall.GetPos () == West)
                cvp [W] = new ValidGhostMoves (false, West);
        }
        return cvp;
    }

    #endregion

    public void Died() {
        ghostMode = GhostMode.Dead;
        this.GetComponent<MeshRenderer> ().material = deadGhost;
        Physics.IgnoreLayerCollision (8, 9, false); //Can Collide with house
        Physics.IgnoreLayerCollision (10, 10, true);
    }

    public void Release() {
        GoUp (ghostHousePos);
        isInHouse = false;
    }

    #region SetGetGhostMode
    public void SetScatter() {
        if (ghostMode != GhostMode.Dead) {
        ghostMode = GhostMode.Scatter;
        AssignColours ();
    }
    }

    public void SetChase() {
        if (ghostMode != GhostMode.Dead) {
            ghostMode = GhostMode.Chase;
            AssignColours ();
        }
    }

    public void SetFrightened() {
        if (ghostMode != GhostMode.Dead) {
            ghostMode = GhostMode.Frightened;
            this.GetComponent<MeshRenderer> ().material = scared;
        }
    }

    public bool IsFrightened() {
        if (ghostMode == GhostMode.Frightened)
            return true;
        else
            return false;
    }

    public bool IsDead() {
        if (ghostMode == GhostMode.Dead)
            return true;
        else
            return false;
    }
    #endregion

    //Sets the positions to the house
    public void ResetMap() {
        isInHouse = true;
        ghostTransform.SetPositionAndRotation (ghostHousePos, Quaternion.Euler (0f, 0f, 0f));
        GoUp (ghostHousePos);
        ghostMode = GhostMode.Chase;
        Physics.IgnoreLayerCollision (8, 9, true);
        Physics.IgnoreLayerCollision (10, 10, true);
    }

    public void Invert() {
        if (direction == Vector2.right)
            GoLeft (ghostTransform.position);
        else if (direction == Vector2.left)
            GoRight (ghostTransform.position);
        else if (direction == Vector2.up)
            GoDown (ghostTransform.position);
        else if (direction == Vector2.down)
            GoUp (ghostTransform.position);
    }
}
