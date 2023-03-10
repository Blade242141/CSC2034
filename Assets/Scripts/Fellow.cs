using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fellow : MonoBehaviour {

    public bool disabled = true;

    public GameManager gm;
    public float speed = 10.0f;

    //Used By Ghosts
    public Vector2 direction = Vector2.zero;

    public Vector3 startPos = Vector3.forward;

    public Transform playerTransform;
    public Rigidbody playerRigidbody;

    float previousH = 0.0f;
    float previousV = 0.0f;

    List<Pellets> pellets = new List<Pellets>();

    private void Start() {
        SetUpPelletList (GameObject.FindGameObjectsWithTag ("Pellet"));
        SetUpPelletList (GameObject.FindGameObjectsWithTag ("PowerUp"));
        SetUpPelletList (GameObject.FindGameObjectsWithTag ("BendPellet"));
        SetUpPelletList (GameObject.FindGameObjectsWithTag ("IntersPellet"));
        SetUpPelletList (GameObject.FindGameObjectsWithTag ("4way"));
        SetUpPelletList (GameObject.FindGameObjectsWithTag ("HousePellet"));
    }

    private void Update() {
        UpdateInput ();
    }

    //Check for input and update direction
    void UpdateInput() {
        float h = Input.GetAxis ("Horizontal");
        float v = Input.GetAxis ("Vertical");

        //SetRotations
        if (h > -0.5 && h < 0.5 && v > -0.5 && v < 0.5) {
            h = Mathf.RoundToInt (previousH);
            v = Mathf.RoundToInt (previousV);
        }
        if (h < -0.5) {
            direction = Vector2.left;
            playerTransform.rotation = Quaternion.Euler (0f, -90f, 0f);
        } else if (h > 0.5) {
            direction = Vector2.right;
            playerTransform.rotation = Quaternion.Euler (0f, 90f, 0f);
        } else if (v < -0.5) {
            direction = Vector2.down;
            playerTransform.rotation = Quaternion.Euler (0f, 180f, 0f);
        } else if (v > 0.5) {
            direction = Vector2.up;
            playerTransform.rotation = Quaternion.Euler (0f, 0f, 0f);
        }

        previousH = h;
        previousV = v;

        Vector3 move = new Vector3 (h, 0.0f, v);
        playerRigidbody.MovePosition (transform.position + move * speed * Time.deltaTime);
    }

    void SetUpPelletList(GameObject [] go) {
        for (int i = 0; i < go.Length; i++) {
            Pellets p = new Pellets (go [i], false);
            pellets.Add (p);
        }
    }

    //If Collide with pellet, increase score and disables pellet
    //If Collide with power up, Change Ghost Mode to Frightened
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Tele") {
            if (direction == Vector2.right)
                playerTransform.SetPositionAndRotation (new Vector3 (0, playerTransform.position.y, playerTransform.position.z), playerTransform.rotation);
            else if (direction == Vector2.left)
                playerTransform.SetPositionAndRotation (new Vector3 (15, playerTransform.position.y, playerTransform.position.z), playerTransform.rotation);
        }

        if (other.tag.Contains("Pellet") || other.tag == "4way") {
            GameObject pellet = other.gameObject;
            foreach (Pellets pell in pellets) {
                if (pell.isSamePellet (pellet) && pell.GetCollected () == false) {
                    //Hide Pellet and set it to collected
                    gm.IncreaseScore ();
                    pellet.GetComponent<MeshRenderer> ().enabled = false;
                    pell.SetCollected(true);
                    break;
                }
            }
        }

        if (other.tag == "PowerUp") {
            //Frightened Mode
            GameObject pellet = other.gameObject;
            foreach (Pellets pell in pellets) {
                if (pell.isSamePellet (pellet) && pell.GetCollected () == false) {
                    //Hide Pellet, set it to collected, scare ghosts
                    gm.IncreaseScore ();
                    pellet.GetComponent<MeshRenderer> ().enabled = false;
                    pell.SetCollected (true);
                    gm.SetGlobalFrightened ();
                    break;
                }
            }
        }
        CheckPelletsLeft ();

        if (other.tag == "Ghost") {
            GhostPathFinding gpf = other.gameObject.GetComponent<GhostPathFinding> ();
            //Collided with ghost
            if (gpf.IsFrightened ()) {
                //Killed Ghost
                gpf.Died ();
            } else if (!gpf.IsDead ()) {
                //Reset positions 
                gm.PlayerDied ();
            }
        }

        if (other.tag == "RandomPowerUp")
            gm.ActivatePowerUp (other.gameObject);
    }
       
    //Check if all pellets have been eaten
    void CheckPelletsLeft() {
        bool pelletsLeft = false;
        int pelletsCollected = 0;
        foreach (Pellets pell in pellets) {
            if (pell.GetCollected () == false)
                pelletsLeft = true;
            else
                pelletsCollected++;
        }
        if (pelletsLeft == false) { 
            //Level Over
            gm.NewLevel ();
        }

        float aThird = (pellets.Count / 3);


        gm.PointCollected (pelletsCollected, aThird, pellets.Count);
    }

    public void ResetMap() {
        playerTransform.SetPositionAndRotation (startPos, Quaternion.Euler (0f, 0f, 0f));
        Physics.IgnoreLayerCollision (8, 9, true);
        Physics.IgnoreLayerCollision (10, 10, true);
    }
}
