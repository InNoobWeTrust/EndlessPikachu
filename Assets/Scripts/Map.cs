using UnityEngine;
using UnityEngine.SceneManagement;
//using System.Collections;

public class Map : MonoBehaviour
{
    //Fields
    public Texture ResetButton;
    public Texture ExitButton;
    int ROW = 10, COL = 10;
    public float aspectRatio = 3 / 4;
    public int minimumPokemon = 10;
    public int[][] MAP;     //pokemon matrix
    public Vec2 POS1;       //position of first selected pokemon
    public Vec2 POS2;       //position of second selected pokemon
    public Vector3[][] POS;     //position of pokemons
    public GameObject[][] gObject;        //game object matrix
    public static int MIN_X;        //horizontal origin of pokemon matrix
    public static int MIN_Y;        //vertical origin of pokemon matrix
    public static int CELL_WIDTH = 32;      //width of a pokemon cell
    public static int CELL_HEIGHT = 48;     //height of a pokemon cell

    //Temporarily object
    private Object prefap_pikachu;

    // Use this for initialization
    void Start()
    {
        Camera.main.orthographicSize = Screen.width;
        Camera.main.aspect = aspectRatio;       //Android build - Nexus 6
        prefap_pikachu = Resources.Load("item");
        if (prefap_pikachu == null) Debug.LogError("Missing prefab!");
        ROW = Random.Range(10, 13);
        while (ROW % 2 != 0) ROW = Random.Range(10, 13);
        gObject = new GameObject[ROW][];
        for (int i = 0; i < ROW; i++)
        {
            gObject[i] = new GameObject[COL];
        }

        LMap(ROW, COL);
        RandomMap();
        if (!CheckMap()) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width / 30, Screen.height - Screen.width / 20, Screen.width / 15, Screen.width / 15), ResetButton))
        {
            //Loads a level
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (GUI.Button(new Rect(Screen.width * 3 / 30, Screen.height - Screen.width / 20, Screen.width / 15, Screen.width / 15), ExitButton))
        {
            //Loads a level
            Application.Quit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float x = mPoint.x;
            float y = mPoint.y;
            //Debug.Log("X " + x + ", Y " + y);
            int mouse_col = (int)((x - MIN_X) / CELL_WIDTH);
            int mouse_row = (int)((y - MIN_Y) / CELL_HEIGHT);
            //Debug.Log(mouse_col + " " + mouse_row);
            if (mouse_row < 1 || mouse_row > ROW - 1 || mouse_col < 1 || mouse_col > COL - 1) return;
            if (POS1 != null && POS1.Collumn == mouse_col && POS1.Row == mouse_row)
            {
                DeSelect();
            }
            else if (POS1 == null && MAP[mouse_row][mouse_col] != -1)
            {
                Select(new Vec2(mouse_row, mouse_col));
            }
            else if (MAP[mouse_row][mouse_col] != -1)
            {
                CheckPair(new Vec2(mouse_row, mouse_col));
            }
        }
    }

    void Select(Vec2 pos)
    {
        POS1 = new Vec2(pos.Row, pos.Collumn);
        POS2 = null;
        //Debug.Log("selected " + POS1.Print());
    }
    void DeSelect()
    {
        POS1 = null;
        POS2 = null;
        //Debug.Log("DeSelect");
    }
    void CheckPair(Vec2 pos)
    {
        if (MAP[POS1.Row][POS1.Collumn] != MAP[pos.Row][pos.Collumn])
        {
            POS1 = null;
            POS2 = null;
            return;
        }
        POS2 = new Vec2(pos.Row, pos.Collumn);
        //Debug.Log("CheckPair " + POS1.Print() + " and " + POS2.Print());
        if (checkTwoPoint(POS1, POS2) != null)
        {
            MAP[POS1.Row][POS1.Collumn] = -1;
            MAP[POS2.Row][POS2.Collumn] = -1;
            Destroy(gObject[POS1.Row][POS1.Collumn]);
            Destroy(gObject[POS2.Row][POS2.Collumn]);
        }
        POS1 = null;
        POS2 = null;
        if (!CheckMap())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    bool CheckMap()
    {
        bool pass = false;
        for (int i1 = 1; i1 < ROW - 1; i1++)
        {
            for (int j1 = 1; j1 < COL - 1; j1++)
            {
                for (int i2 = 1; i2 < ROW - 1; i2++)
                {
                    for (int j2 = 1; j2 < COL - 1; j2++)
                    {
                        if (i1 == i2 && j1 == j2) continue;
                        if (MAP[i1][j1] == -1 || MAP[i2][j2] == -1) continue;
                        if (MAP[i1][j1] != MAP[i2][j2]) continue;
                        if (checkTwoPoint(new Vec2(i1, j1), new Vec2(i2, j2)) != null)
                        {
                            pass = true;
                            goto Exit;
                        }
                    }
                }
            }
        }
        Exit:
        //Debug.Log("Check map result: " + pass);
        return pass;
    }

    void AddPokemon(int type, Vector3 pos, int width, int height, ref GameObject pokemon)
    {
        pokemon = Instantiate(prefap_pikachu) as GameObject;
        pokemon.transform.parent = this.transform;
        pokemon.transform.position = pos;
        Sprite sprite = Resources.Load("h" + type, typeof(Sprite)) as Sprite;
        pokemon.GetComponent<SpriteRenderer>().sprite = sprite;
        pokemon.transform.localScale = new Vector3(Mathf.Abs(width * 1.0f / sprite.bounds.size.x), Mathf.Abs(height * 1.0f / sprite.bounds.size.y), 1);
    }

    public void LMap(int row, int col)
    {
        ROW = row;
        COL = col;
        CELL_WIDTH = Screen.width / (COL - 2);
        CELL_HEIGHT = CELL_WIDTH * 4 / 3;
        //CELL_HEIGHT = Screen.height / ROW;
        //CELL_WIDTH = CELL_HEIGHT * 3 / 4;
        //Debug.Log("cellWidth " + CELL_WIDTH + ", cellHeight " + CELL_HEIGHT);

        MAP = new int[ROW][];
        POS = new Vector3[ROW][];

        MIN_X = -COL * CELL_WIDTH / 2;
        MIN_Y = -ROW * CELL_HEIGHT / 2;
        //Debug.Log("minX = " + MIN_X + ", minY = " + MIN_Y);

        for (int i = 0; i < ROW; i++)
        {
            MAP[i] = new int[COL];
            POS[i] = new Vector3[COL];
            for (int j = 0; j < COL; j++)
            {
                if (i == 0 || i == ROW - 1 || j == 0 || j == COL - 1)
                    MAP[i][j] = -1;

                POS[i][j] = new Vector3(0, 0, 0);
                POS[i][j].x = MIN_X + j * CELL_WIDTH + CELL_WIDTH / 2;
                POS[i][j].y = MIN_Y + i * CELL_HEIGHT + CELL_HEIGHT / 2;
                POS[i][j].z = 0;
            }
        }
        int countBlocker = Random.Range(0, (ROW - 2) * (COL - 2) - minimumPokemon);
        while (countBlocker % 2 != 0) countBlocker = Random.Range(0, (ROW - 2) * (COL - 2) - 10);
        while (countBlocker > 0)
        {
            int i = Random.Range(0, ROW - 1), j = Random.Range(0, COL - 1);
            if (MAP[i][j] == -1) continue;
            if ((MAP[i][j] = Random.Range(-1, 0)) == -1) countBlocker--;
        }
    }

    void RandomMap()
    {
        int count = 0;
        for (int i = 1; i < ROW - 1; i++)
            for (int j = 1; j < COL - 1; j++)
                if (MAP[i][j] != -1)
                    count++;

        if (count % 2 == 1)
        {
            Debug.LogError("There is a lonely pokemon here!");
            return;
        }

        int[] pool = new int[count];

        for (int i = 0; i < count / 2; i++)
            pool[i] = Random.Range(0, 36);
        for (int i = count / 2; i < count; i++)
            pool[i] = pool[count - 1 - i];


        for (int i = 0; i < count / 2; i++)
        {
            int index1 = Random.Range(0, count);
            int index2 = Random.Range(0, count);
            if (index1 == index2) break;
            int temp = pool[index1];
            pool[index1] = pool[index2];
            pool[index2] = temp;
        }

        for (int i = 0; i < ROW; i++)
        {
            for (int j = 0; j < COL; j++)
            {
                if (MAP[i][j] != -1)
                {
                    MAP[i][j] = pool[--count];
                    AddPokemon(pool[count], POS[i][j], CELL_WIDTH, CELL_HEIGHT, ref gObject[i][j]);
                }
            }
        }
        if (count > 0) Debug.LogError("Random fail!");
    }

    bool checkRow(int col1, int col2, int row, bool considerStartPoint = true, bool considerEndPoint = true)
    {
        int min = Mathf.Min(col1, col2);
        int max = Mathf.Max(col1, col2);
        if (max == min)
        {
            if (considerStartPoint || considerEndPoint)
            {
                if (MAP[row][min] != -1) return true;
                else return false;
            }
            else return true;
        }
        for (int col = min; col <= max; col++)
        {
            if (!considerStartPoint && col == col1) continue;
            if (!considerEndPoint && col == col2) continue;
            if (MAP[row][col] != -1)
                //blocked path
                return false;
        }
        return true;
    }

    bool checkCol(int row1, int row2, int col, bool considerStartPoint = true, bool considerEndPoint = true)
    {
        // find point have column max and min
        int min = Mathf.Min(row1, row2);
        int max = Mathf.Max(row1, row2);
        if (max == min)
        {
            if (considerStartPoint || considerEndPoint)
            {
                if (MAP[min][col] != -1) return true;
                else return false;
            }
            else return true;
        }
        for (int row = min; row <= max; row++)
        {
            if (!considerStartPoint && row == row1) continue;
            if (!considerEndPoint && row == row2) continue;
            if (MAP[row][col] != -1)
                //blocked path
                return false;
        }
        return true;
    }

    int checkRectRow(Vec2 v1, Vec2 v2)
    {
        // find position which has min and max collumn
        Vec2 pMinCol = v1, pMaxCol = v2;
        if (v1.Collumn > v2.Collumn)
        {
            pMinCol = v2;
            pMaxCol = v1;
        }
        for (int col = pMinCol.Collumn; col <= pMaxCol.Collumn; col++)
        {
            // check two line
            if (col == pMinCol.Collumn)
            {
                if (checkCol(pMinCol.Row, pMaxCol.Row, pMinCol.Collumn, false, true)
                && checkRow(pMinCol.Collumn, pMaxCol.Collumn, pMaxCol.Row, true, false))
                {

                    //Debug.Log("Corner row");
                    //Debug.Log("(" + pMinCol.Row + "," + pMinCol.Collumn + ") -> ("
                    //        + pMaxCol.Row + "," + pMinCol.Collumn
                    //        + ") -> (" + pMaxCol.Row + "," + pMaxCol.Collumn + ")");
                    // if two line is true return column
                    return col;
                }
                continue;
            }
            else if (col == pMaxCol.Collumn)
            {
                if (checkRow(pMinCol.Collumn, pMaxCol.Collumn, pMinCol.Row, false, true)
                && checkCol(pMinCol.Row, pMaxCol.Row, pMaxCol.Collumn, true, false))
                {

                    //Debug.Log("Rect row");
                    //Debug.Log("(" + pMinCol.Row + "," + pMinCol.Collumn + ") -> ("
                    //        + pMinCol.Row + "," + pMaxCol.Collumn
                    //        + ") -> (" + pMaxCol.Row + "," + pMaxCol.Collumn + ")");
                    // if two line is true return column
                    return col;
                }
                continue;
            }
            // check three line
            if (checkRow(pMinCol.Collumn, col, pMinCol.Row, false, true)
                && checkCol(pMinCol.Row, pMaxCol.Row, col, true, true)
                && checkRow(col, pMaxCol.Collumn, pMaxCol.Row, true, false))
            {

                //Debug.Log("Rect row");
                //Debug.Log("(" + pMinCol.Row + "," + pMinCol.Collumn + ") -> ("
                //        + pMinCol.Row + "," + col + ") -> (" + pMaxCol.Row + "," + col
                //        + ") -> (" + pMaxCol.Row + "," + pMaxCol.Collumn + ")");
                // if three line is true return column
                return col;
            }
        }
        //fail to find a collumn to connect
        return -1;
    }

    int checkRectCol(Vec2 v1, Vec2 v2)
    {
        // find position which has min and max collumn
        Vec2 pMinRow = v1, pMaxRow = v2;
        if (v1.Row > v2.Row)
        {
            pMinRow = v2;
            pMaxRow = v1;
        }
        for (int row = pMinRow.Row; row <= pMaxRow.Row; row++)
        {
            // check two line
            if (row == pMinRow.Row)
            {
                if (checkRow(pMinRow.Collumn, pMaxRow.Collumn, pMinRow.Row, false, true)
                    && checkCol(pMinRow.Row, pMaxRow.Row, pMaxRow.Collumn, true, false))
                {

                    //Debug.Log("Corner row");
                    //Debug.Log("(" + pMinRow.Row + "," + pMinRow.Collumn + ") -> ("
                    //        + pMinRow.Row + "," + pMaxRow.Collumn
                    //        + ") -> (" + pMaxRow.Row + "," + pMaxRow.Collumn + ")");
                    // if two line is true return row
                    return row;
                }
                continue;
            }
            else if (row == pMaxRow.Row)
            {
                if (checkCol(pMinRow.Row, pMaxRow.Row, pMinRow.Collumn, false, true)
                && checkRow(pMinRow.Collumn, pMaxRow.Collumn, pMaxRow.Row, true, false))
                {

                    //Debug.Log("Rect row");
                    //Debug.Log("(" + pMinRow.Row + "," + pMinRow.Collumn + ") -> ("
                    //        + pMinRow.Row + "," + pMaxRow.Collumn
                    //        + ") -> (" + pMaxRow.Row + "," + pMaxRow.Collumn + ")");
                    // if two line is true return row
                    return row;
                }
                continue;
            }
            // check three line
            if (checkCol(pMinRow.Row, row, pMinRow.Collumn, false, true)
                && checkRow(pMinRow.Collumn, pMaxRow.Collumn, row, true, true)
                && checkCol(row, pMaxRow.Row, pMaxRow.Collumn, true, false))
            {

                //Debug.Log("Rect col");
                //Debug.Log("(" + pMinRow.Row + "," + pMinRow.Collumn + ") -> ("
                //        + row + "," + pMinRow.Collumn + ") -> (" + row + "," + pMaxRow.Collumn
                //        + ") -> (" + pMaxRow.Row + "," + pMaxRow.Collumn + ")");
                //if three line is true return row
                return row;
            }
        }
        //fail to find a row to connect
        return -1;
    }

    int checkMoreRow(Vec2 v1, Vec2 v2, int step)
    {
        // find position which has min collumn
        Vec2 pMinCol = v1, pMaxCol = v2;
        if (v1.Collumn > v2.Collumn)
        {
            pMinCol = v2;
            pMaxCol = v1;
        }
        // find line and collumn to begin
        int col = pMaxCol.Collumn + step;
        int row = pMinCol.Row;
        if (step == -1)
        {
            col = pMinCol.Collumn + step;
            row = pMaxCol.Row;
        }
        // check more
        if (checkRow(pMinCol.Collumn, pMaxCol.Collumn, row, row == pMinCol.Row ? false : true, row == pMaxCol.Row ? false : true))
        {
            while (MAP[pMinCol.Row][col] == -1
                    && MAP[pMaxCol.Row][col] == -1 && col >= 0 && col <= COL)
            {
                if (checkCol(pMinCol.Row, pMaxCol.Row, col))
                {

                    //Debug.Log("TH row " + step);
                    //Debug.Log("(" + pMinCol.Row + "," + pMinCol.Collumn + ") -> ("
                    //        + pMinCol.Row + "," + col + ") -> (" + pMaxCol.Row + "," + col
                    //        + ") -> (" + pMaxCol.Row + "," + pMaxCol.Collumn + ")");
                    return col;
                }
                col += step;
            }
        }
        return -1;
    }

    int checkMoreCol(Vec2 v1, Vec2 v2, int step)
    {
        // find position which has min row
        Vec2 pMinRow = v1, pMaxRow = v2;
        if (v1.Row > v2.Row)
        {
            pMinRow = v2;
            pMaxRow = v1;
        }
        // find line and row to begin
        int row = pMaxRow.Row + step;
        int col = pMinRow.Collumn;
        if (step == -1)
        {
            row = pMinRow.Row + step;
            col = pMaxRow.Collumn;
        }
        // check more
        if (checkCol(pMinRow.Row, pMaxRow.Row, col, col == pMinRow.Collumn ? false : true, col == pMaxRow.Collumn ? false : true))
        {
            while (MAP[row][pMinRow.Collumn] == -1
                    && MAP[row][pMaxRow.Collumn] == -1 && row >= 0 && row <= ROW)
            {
                if (checkRow(pMinRow.Collumn, pMaxRow.Collumn, row))
                {

                    //Debug.Log("TH col " + step);
                    //Debug.Log("(" + pMinRow.Row + "," + pMinRow.Collumn + ") -> ("
                    //        + row + "," + pMinRow.Collumn + ") -> (" + row + "," + pMaxRow.Collumn
                    //        + ") -> (" + pMaxRow.Row + "," + pMaxRow.Collumn + ")");
                    return row;
                }
                row += step;
            }
        }
        return -1;
    }

    MyLine checkTwoPoint(Vec2 p1, Vec2 p2)
    {
        // check line with row
        if (p1.Row == p2.Row)
        {
            if (checkRow(p1.Collumn, p2.Collumn, p1.Row, false, false))
            {
                return new MyLine(p1, p2);
            }
        }
        // check line with collumn
        if (p1.Collumn == p2.Collumn)
        {
            if (checkCol(p1.Row, p2.Row, p1.Collumn, false, false))
            {
                return new MyLine(p1, p2);
            }
        }

        int t = -1; // t is column find

        // check in rectangle with x
        if ((t = checkRectRow(p1, p2)) != -1)
        {
            return new MyLine(new Vec2(p1.Row, t), new Vec2(p2.Row, t));
        }

        // check in rectangle with y
        if ((t = checkRectCol(p1, p2)) != -1)
        {
            return new MyLine(new Vec2(t, p1.Collumn), new Vec2(t, p2.Collumn));
        }
        // check more right
        if ((t = checkMoreRow(p1, p2, 1)) != -1)
        {
            return new MyLine(new Vec2(p1.Row, t), new Vec2(p2.Row, t));
        }
        // check more left
        if ((t = checkMoreRow(p1, p2, -1)) != -1)
        {
            return new MyLine(new Vec2(p1.Row, t), new Vec2(p2.Row, t));
        }
        // check more down
        if ((t = checkMoreCol(p1, p2, 1)) != -1)
        {
            return new MyLine(new Vec2(t, p1.Collumn), new Vec2(t, p2.Collumn));
        }
        // check more up
        if ((t = checkMoreCol(p1, p2, -1)) != -1)
        {
            return new MyLine(new Vec2(t, p1.Collumn), new Vec2(t, p2.Collumn));
        }
        return null;
    }
}

public class Vec2
{
    public int Row;
    public int Collumn;

    public Vec2()
    {
        Row = 0;
        Collumn = 0;
    }

    public Vec2(int r, int c)
    {
        Row = r;
        Collumn = c;
    }
    public Vec2(Vector2 vec)
    {
        Row = (int)vec.y;
        Collumn = (int)vec.x;
    }
    public Vec2(Vec2 vec)
    {
        Row = vec.Row;
        Collumn = vec.Collumn;
    }
    static int row, collumn;
    static public int FastDistance(Vec2 v1, Vec2 v2)
    {
        row = Mathf.Abs(v1.Row - v2.Row);
        collumn = Mathf.Abs(v1.Collumn - v2.Collumn);
        if (row > collumn) return row;
        return collumn;
    }

    public string Print()
    {
        string r = "(" + Row + "," + Collumn + ")";
        return r;
    }
}

public class MyLine
{
    public Vec2 p1;
    public Vec2 p2;

    public MyLine(Vec2 p1, Vec2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public override string ToString()
    {
        string str = "(" + p1.Row + "," + p1.Collumn + ") and (" + p2.Row + "," + p2.Collumn + ")";
        return str;
    }
}