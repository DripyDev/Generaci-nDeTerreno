using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    
    //Sensibilidad del raton
    public float sensibilidadRaton = 100f;
    //Nivel de rotacion
    float xRotation = 0f;
    float yRotation = 0f;
    //Posicion de la camara
    public Transform cameraPos;
    ///<summary>True: Movimiento recto. False: Movimiento en funcion de a donde miramos</summary>
    public bool movimientoRecto = false;

/*
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton * Time.deltaTime;

        //Restamos porque unity invierte los ejes
        xRotation -= mouseY;
        //Limite de rotacion de la camara
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Las rotaciones en unity son a traves de quaterniones
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        cameraPos.Rotate(Vector3.up * mouseX);
    }
*/

    //Funcion para mover la camara por teclado
    void movimientoCamara(){
        //NOTA: Mirar como hacer que la camara vaya en funcion de Time.timeScale=1. Que sea independiente a la velocidad del mundo
        //-------------ROTACION--------------------------
        //Click derecho
        if (Input.GetMouseButton(1)) {
            Cursor.lockState = CursorLockMode.Locked;
            //float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton * Time.fixedUnscaledDeltaTime;
            //float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton * Time.fixedUnscaledDeltaTime;
            float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton * Time.deltaTime;

            //Restamos porque unity invierte los ejes
            xRotation -= mouseY;
            //No se porque haria falta el clamped para una camara de movimiento libre
            //xRotation solo puede ser entre -90 y 90 grados
            //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            yRotation += mouseX;
            //yRotation = Mathf.Clamp(-90f, yRotation ,90f);

            //Las rotaciones en unity son a traves de quaterniones
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            //cameraPos.Rotate(Vector3.up * mouseX);
        }
        else {
            Cursor.lockState = CursorLockMode.None;
        }
        //-------------MOVIMIENTO-------------------------
        if(Input.GetKeyDown(KeyCode.LeftShift))
            movimientoRecto = movimientoRecto? false:true;
        //Movimientos rectos
        if(movimientoRecto){
            //Avanzamos
            if(Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
                cameraPos.position += new Vector3(0f,0f,1f);
            //Retrocedemos
            if(Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
                cameraPos.position += new Vector3(0f,0f,-1f);
            //Izquierda
            if(Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
                cameraPos.position += new Vector3(-1f,0f,0f);
            //Derecha
            if(Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
                cameraPos.position += new Vector3(1f,0f,0f);
        }
        else{
            //Avanzamos
            if(Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
                cameraPos.position += this.transform.forward;//Avance en direccion a donde miramos
            //Retrocedemos
            if(Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow))
                cameraPos.position -= this.transform.forward;
            //Izquierda
            if(Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow))
                cameraPos.position -= this.transform.right;
            //Derecha
            if(Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow))
                cameraPos.position += this.transform.right;
        }
        //Arriba
        if(Input.GetKey("space")){
            cameraPos.localPosition += new Vector3(0f,1f,0f);
        }
        //Abajo
        if(Input.GetKey(KeyCode.LeftControl)){
            cameraPos.localPosition += new Vector3(0f,-1f,0f);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movimientoCamara();
    }
}
