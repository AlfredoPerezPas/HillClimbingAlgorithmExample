using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SudokuChest : MonoBehaviour
{

    public GameObject[] Boxes;
    // Start is called before the first frame update
    void Start()
    {
        //Boxes = new GameObject[81];

        //GetBoxes();

        SetBoxColor(5, 5, Color.green);

    }

    private void GetBoxes() {

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Boxes[0] = transform.GetChild(0).GetChild(0).gameObject;
                }
            }
        }
    }

    int TransformMatrixToLineal(int i, int j) {
        int pos = (((i - 1) * 9) + j) - 1;
        if (pos > 81 || pos < 0) {
            pos = 0;
        }
        return pos;
    }

    void SetBoxColor(int i, int j, Color color) {
        int pos = TransformMatrixToLineal(i,j);
        ColorBlock cb = Boxes[pos].GetComponent<Button>().colors;
        cb.normalColor = color;
        Boxes[pos].GetComponent<Button>().colors = cb;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Evolution Sumulatos YouTube
    /*
    https://www.researchgate.net/publication/319886025_b-Hill_Climbing_Algorithm_for_Sudoku_Game 
    */
}
