using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    public GameObject canonBase;
    public GameObject canon;
    public GameObject canonBall;
    public Transform target;
    public Transform spawnPoint;
    public int randomIterations = 5;
    public int speedIterations = 1;
    int iterations;
    float horizontalAngle = 0;
    float verticalAngle = 0;
    float ballImpulse = 5;
    float rotationSpeed = 10;
    public float marginClimb = 10;

    int pullSize = 26;
    struct BallInfo{
        public float baseAngle;
        public float canonAngle;
        public float impulse;
        public float bestScore;
        public bool isFinish;
        public float upPosition;
        public bool isDown;

    }

    int actualBall = 0;
    GameObject[] canonBallPull;
    BallInfo[] ballsInfo;
    BallInfo bestBall;
    BallInfo bestIterationBall;

    bool isIAFinish;

    enum IAState {  RandomShoot, CalculationRandomShoot,
                    MarginShoot, CalculationMargin,
                    EndMargin, Sleep};

    IAState iaState = IAState.RandomShoot;


    void Awake() {
        GenerateBallPull(pullSize);
        bestBall = new BallInfo();
        bestIterationBall = new BallInfo();
        bestBall.bestScore = GetScore(canonBallPull[0]);
        Fast(speedIterations);
    }

    // Start is called before the first frame update
    void Start() {
        //ShotMarginValues(bestBall);
    }

    // Update is called once per frame
    void Update() {

        if (!isIAFinish) {
            IA();
        }

        else {
            KeyBoardController();
        }

        ShowRayGuide();
    }

    void IA() {
        float actualBest = bestBall.bestScore;
        if (iterations <= randomIterations) {

            // Primero hacemos un tiro aleatorio
            if (iaState == IAState.RandomShoot) {
                IARandomShotBall();
                iaState = IAState.CalculationRandomShoot;
                iterations++;
            }
            // Calculamos la puntuacion segun las fisicas
            else if (iaState == IAState.CalculationRandomShoot) {
                if (ShootIsFinishAndBest(0)) {
                    iaState = IAState.MarginShoot;
                }
            }
            // Si es mejor que ninguna, disparamos los valores proximos de esta.
            else if (iaState == IAState.MarginShoot) {
                FindMarginValues(bestBall);
                ShotMarginValues(bestBall);
                iaState = IAState.CalculationMargin;
            }

            // Calculamos los valores proximos.
            else if (iaState == IAState.CalculationMargin) {
                if (AllShootsIsFinishAndBest()) {
                    if (actualBest == bestBall.bestScore) {
                        iaState = IAState.RandomShoot;
                    }
                    else {
                        iaState = IAState.MarginShoot;
                    }
                }
            }

            else if (iaState == IAState.EndMargin) {

            }
        }
        else{
            isIAFinish = true;
            NormalSpeed();
        }
    }

    // Dispara de forma aleatoria una bala con un margen.
    void IARandomShotBall() {
        IAShotBall(Random.Range(0.0f,359), Random.Range(0.0f, 100.0f), Random.Range(0, 25.0f), 0);        
    }

    // Dispara y guarda los valores de los margenes generados
    void ShotMarginValues(BallInfo ball) {
        //FindMarginValues(ball);
        for (int i = 0; i < pullSize; i++) {
            IAShotBall(ballsInfo[i].baseAngle, ballsInfo[i].canonAngle, ballsInfo[i].impulse, i);
        }
    }

    // Devuelve si todos los jugadores han terminado
    /*
    bool RefreshAllScores(){
        bool _allBallsFinish = true;
        for (int i = 0; i < pullSize; i++) {

            // Deteccion de caida
            if (ballsInfo[i].upPosition < canonBallPull[i].transform.position.y) {
                ballsInfo[i].isDown = true;
            }
            else {
                ballsInfo[i].isDown = false;
            }

            // La puntuacion Mejora. Si no mejora la puntuacion y cae, la pelota se considera que ya llega a su maxima puntuacion.
            if (ballsInfo[i].bestScore > GetScore(canonBallPull[i])) {
                ballsInfo[i].bestScore = GetScore(canonBallPull[i]);
            }
            else if (ballsInfo[i].isDown) {
                _allBallsFinish = false;
            }

        }

        float bestScore = ballsInfo[0].bestScore;
        for (int i = 0; i < pullSize; i++) {
            if (bestScore > ballsInfo[i].bestScore) {
                bestScore = ballsInfo[i].bestScore;

                if (bestBall.bestScore > bestScore) {
                    bestBall = ballsInfo[i];
                }

            }
        }

        if (_allBallsFinish) {
            Debug.Log("bestScore  --------------------> " + bestBall.bestScore);
            return true;
        }

        return false;
    }
    */

    bool AllShootsIsFinishAndBest() {
        bool isFinish = true;
        for (int i = 0; i < pullSize; i++) {
            if (!ShootIsFinishAndBest(i)) {
                isFinish = false;
            }
        }
        return isFinish;
    }

    bool ShootIsFinishAndBest(int listPosition) {

        //Debug.Log("Impulse Random " + ballsInfo[0].impulse);
        bool isFinish = false;
        // Deteccion de caida
        if (ballsInfo[listPosition].upPosition > canonBallPull[listPosition].transform.position.y) {
            ballsInfo[listPosition].isDown = true;           
        }
        else {
            ballsInfo[listPosition].isDown = false;
        }

        // Si la puntuacion Mejora. 
        if (ballsInfo[listPosition].bestScore > GetScore(canonBallPull[listPosition])) {
            ballsInfo[listPosition].bestScore = GetScore(canonBallPull[listPosition]);
        }
        // Si  la puntuacion no mejora, la pelota cae se considera que ya llega a su maxima puntuacion.
        else if (ballsInfo[listPosition].isDown) {            
            if (bestBall.bestScore > ballsInfo[listPosition].bestScore) {
                bestBall = ballsInfo[listPosition];
                DebugBest();
            }
            isFinish = true;            
        }    
        return isFinish;
    }

    void SetBestScores() {


    }
    // Devuelve la distancia de una bala con el target
    float GetScore(GameObject ball) {
        return Vector3.Distance(ball.transform.position, target.position);
    }

    void DebugBest() {
        Debug.Log(  "score: " + bestBall.bestScore +
                    "  baseAngle: " + bestBall.baseAngle +
                    "  canonAngle: " + bestBall.canonAngle +
                    "  impulse: " + bestBall.impulse);
    }

    // Manejo Manual del cañon para pruebas
    void KeyBoardController() {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            
            verticalAngle -= rotationSpeed * Time.deltaTime;
            RotateCanonTo(verticalAngle);

        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalAngle += rotationSpeed * Time.deltaTime;
            RotateCanonTo(verticalAngle);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalAngle += rotationSpeed * Time.deltaTime;
            RotateCanonBaseTo(horizontalAngle);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalAngle -= rotationSpeed * Time.deltaTime;
            RotateCanonBaseTo(horizontalAngle);
        }
        if (Input.GetKey(KeyCode.Z)) {
            ballImpulse += 1;

        }
        if (Input.GetKey(KeyCode.X)) {
            ballImpulse -= 1;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShotBall(canonBallPull[0], ballImpulse);
        }

    }

    // Setea todos los BallInfo Y les añade el margen deseado en todas las dimensiones.
    void FindMarginValues(BallInfo ball) {

        // x x x     0 1 2 
        // x x x ==> 3 4 5 ==> 
        // x x x     6 7 8            
        ballsInfo[0] = ModifieBallInfo(ball, 1,  1,  1);
        ballsInfo[1] = ModifieBallInfo(ball, 1,  1,  0);
        ballsInfo[2] = ModifieBallInfo(ball, 1,  1, -1);
        ballsInfo[3] = ModifieBallInfo(ball, 1,  0,  1);
        ballsInfo[4] = ModifieBallInfo(ball, 1,  0,  0);
        ballsInfo[5] = ModifieBallInfo(ball, 1,  0, -1);
        ballsInfo[6] = ModifieBallInfo(ball, 1, -1,  1);
        ballsInfo[7] = ModifieBallInfo(ball, 1, -1,  0);
        ballsInfo[8] = ModifieBallInfo(ball, 1, -1, -1);

        // x x x       9 10 11
        // x o x ==>  12    13 
        // x x x      14 15 16
        ballsInfo[9] = ModifieBallInfo(ball, 0, 1, 1);
        ballsInfo[10] = ModifieBallInfo(ball, 0, 1, 0);
        ballsInfo[11] = ModifieBallInfo(ball, 0, 1, -1);
        ballsInfo[12] = ModifieBallInfo(ball, 0, 0, 1);
        // ballsInfo[13] = ModifieBallInfo(ball, 0, 0, 0); <-- center, dont modifier
        ballsInfo[13] = ModifieBallInfo(ball, 0, 0, -1);
        ballsInfo[14] = ModifieBallInfo(ball, 0, -1, 1);
        ballsInfo[15] = ModifieBallInfo(ball, 0, -1, 0);
        ballsInfo[16] = ModifieBallInfo(ball, 0, -1, -1);

        // x x x      17 18 19 
        // x x x ==>  20 21 22
        // x x x      23 24 25
        ballsInfo[17] = ModifieBallInfo(ball, -1, 1, 1);
        ballsInfo[18] = ModifieBallInfo(ball, -1, 1, 0);
        ballsInfo[19] = ModifieBallInfo(ball, -1, 1, -1);
        ballsInfo[20] = ModifieBallInfo(ball, -1, 0, 1);
        ballsInfo[21] = ModifieBallInfo(ball, -1, 0, 0);
        ballsInfo[22] = ModifieBallInfo(ball, -1, 0, -1);
        ballsInfo[23] = ModifieBallInfo(ball, -1, -1, 1);
        ballsInfo[24] = ModifieBallInfo(ball, -1, -1, 0);
        ballsInfo[25] = ModifieBallInfo(ball, -1, -1, -1);
    }

    // Modifica un BallInfo con el margen establecido.
    BallInfo ModifieBallInfo(BallInfo ball, int incrementBaseAngle, int incrementCanonAngle, int incrementImpulse) {

        if (incrementBaseAngle > 0)       ball.baseAngle += marginClimb;
        else if (incrementBaseAngle < 0)  ball.baseAngle -= marginClimb;

        if (incrementCanonAngle > 0)      ball.canonAngle += marginClimb;
        else if (incrementCanonAngle < 0) ball.canonAngle -= marginClimb;

        if (incrementImpulse > 0)         ball.impulse += marginClimb;
        else if (incrementImpulse < 0)    ball.impulse -= marginClimb;

        return ball;
    }

    // Dispara solo una bala, y guarda sus atributos en el BallInfo
    void IAShotBall(float baseAngle, float canonAngle, float impulse, int ballListPosition) {
       
        ballsInfo[ballListPosition].baseAngle = baseAngle;
        ballsInfo[ballListPosition].canonAngle = canonAngle;
        ballsInfo[ballListPosition].impulse = impulse;
        ballsInfo[ballListPosition].bestScore = GetScore(canonBallPull[ballListPosition]);
        //Debug.Log("impulse  --> " + impulse + "   ¿?¿?   " + ballsInfo[ballListPosition].impulse);
        RotateCanonBaseTo(baseAngle);
        RotateCanonTo(canonAngle);
        ShotBall(canonBallPull[ballListPosition], impulse);        
    }

    // Rotacion de la base del cañon --> Horizontal
    void RotateCanonBaseTo(float Angle) {
        canonBase.transform.localRotation = Quaternion.Euler(0, Angle, 0);
    }

    // Totacion del cañon --> Vetical
    void RotateCanonTo(float Angle)
    {
        canon.transform.localRotation = Quaternion.Euler(Angle, 0, 0);
    }

    // Genera una pull con el numero suficiente, para ver todas las variaciones
    void GenerateBallPull(int pullSize) {
        canonBallPull = new GameObject[pullSize];
        ballsInfo = new BallInfo[pullSize];
        for (int i = 0; i < pullSize; i++) {
            canonBallPull[i] = Instantiate(canonBall, spawnPoint);
        }    

    }

    // Disparo fisico de una bala, con el impulso que se le indique
    void ShotBall(GameObject ball, float impulse) {

        ball.transform.localPosition = Vector3.zero;
        ball.transform.localRotation = Quaternion.Euler(0, 0, 0);
        ball.GetComponent<Rigidbody>().isKinematic = false;

        stopForces(ball);

        ball.GetComponent<Rigidbody>().AddForce((spawnPoint.position - canon.transform.position).normalized * impulse, ForceMode.Impulse);

    }

    // Para la fuerza de una bala para que no repercuta al volver a dispararse
    void stopForces(GameObject ball) {
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    // Guia de direccion de las balas.
    void ShowRayGuide() {
        Debug.DrawRay(spawnPoint.position, ( spawnPoint.position - canon.transform.position) * ballImpulse, Color.magenta);
    }

    void Stop() {
        Time.timeScale = 0;
    }

    void Fast(int i) {
        Time.timeScale = i;
    }

    void NormalSpeed() {
        Time.timeScale = 1;
    }

}
