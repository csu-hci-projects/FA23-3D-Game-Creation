using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    Vector2 mouseLook;
    Vector2 smoothV;
    public float sensitivity = 2.0f;
    public float smoothing = 2.0f;

    bool cameraActive;

    GameObject player;
    // Use this for initialization
    void Start() {
        player = this.transform.parent.gameObject;
        cameraActive = true;
    }

    // Update is called once per frame
    void Update() {
        var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));  //Determina a rotação 


        smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
        mouseLook += smoothV;

        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);  //Impede que o jogador rode a camera 360 graus da vertical
        mouseLook += smoothV;



        transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);    //Roda a camara
        player.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, player.transform.up); //Roda o jogador

        checkMouse(); //Esconde o mouse
    }

    private void checkMouse() {
        if (cameraActive)
        {
            Cursor.visible = !(cameraActive);
            Screen.lockCursor = cameraActive;
        } }

}
