//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{

    const int kinder = 5;                                 //ピースの種類
    int Itemkind;                                         //アイテムの情報
    const int Itemmax = 8;                                //アイテムの種類数(-5)
    bool Itemcheck = false;                               //そのターン、アイテムを作ったかのチェック

    public Text Chain;                                  //連鎖数のテキスト
    public Text Score;                                  //スコアのテキスト
    public Text Change;                                 //色変え回数のテキスト

    public static int changecount = 2;

    [SerializeField]
    private Sprite[] texture = new Sprite[8];           //ピースの色をセットするとこ
    [SerializeField]
    private Image ball;                                 //生成するピース
    [SerializeField]
    private GameObject Canvas;                          //親となるキャンバス
    [SerializeField]
    private GameObject SM;


    float tm;                                           //デリートピースで遅らせるやつ
    int matchcount;                                     //マッチからフィルかデリートか判別するやつ
    float time = 0;										//フィルピースで遅らせるやつ（上と一緒でもいけそう？）

    public enum GameState
    {
        //Idle,
        Changecolor,
        MatchCheck,
        DeletePiece,
        FillPiece,
        Pause,
    }
    public GameState currentState = GameState.MatchCheck;       //GameState 状態をとっている

    public static bool t1, t2;                                      //現状チェンジカラーからの遷移につかっている やめたい

    enum color
    {                                       //０が赤、１が緑、２が黄色、３が青 4を仮に白
        red,
        green,
        yellow,
        bule
    };

    public Image next;							//ネクストの色を表示
    public Image nextnext;
    public bool[,] deleteflag = new bool[6, 6];    //盤面のデリートフラグ
    public int[,] kindinfo = new int[6, 6];        //盤面の色情報

    public static int chaincount;                         //連鎖数のカウント
    private bool chainkeep;                         //一連の連鎖かどうかの判別
    public static int score;                              //スコアです
    private Skill SK;

    public static int nc = 1;							//消した数数えた
    private bool onetime = true;                        //一回の連鎖中であるかの判断 oneturnncmaxに使う
    public static int oneturnncmax = 0;                 //一回の連鎖での消した個数の最大 アイテム生成に使う

    public static int changekind;						//ネクストの色情報をいれるとこ
    private int next_change_kind;
    private bool nullcontinu = false;           //ぼくじゃないので正確にはわからない
    public static int countc = 0;				//マッチでcountを足し合わせるやつ
    int kind;                                   //色情報
    private float posX, posY;               //ボールの幅と高さを取って居ます。
    private Image[,] board = new Image[6, 6];       //盤面のImage情報

    private bool[,] checkflag = new bool[6, 6];	//盤面のチェックフラグ（たぶんピースにもたせた方がいい）


    bool scorekeep = true;
    public static bool startcheck = true;
    public bool statechange = false;
    private bool Endgame = false;                //残りがなくなったかどうか判定します
    float tt;




    //ネクストのセットのみ
    void Awake()
    {

        Chain = GameObject.Find("ChainCount").GetComponent<Text>();
        changekind = Random.Range(0, kinder);
        next_change_kind = Random.Range(0, kinder);
        next.GetComponent<Image>().sprite = texture[changekind];
        nextnext.GetComponent<Image>().sprite = texture[next_change_kind];
    }


    //最初の盤面クリエイト ここからマッチ(初期状態)に飛ばして整理
    public void Start()
    {
        tt = 0;
        chaincount = 0;
        score = 0;
        startcheck = true;
        changecount = 20;
        SK = GameObject.Find("Skill").gameObject.GetComponent<Skill>();
        Chain.text = chaincount.ToString() + "れんさ";
        Score.text = score.ToString("D6");
        Change.text = changecount.ToString("D2");
        RectTransform s = ball.GetComponent<RectTransform>();

        //posX = s.sizeDelta.x;
        //posY = s.sizeDelta.y;
        posX = 120;
        posY = 120;
        for (int x = 5; x >= 0; x--)
        {
            for (int y = 5; y >= 0; y--)
            {
                Createpiece(x, y);
            }
        }
        currentState = GameState.MatchCheck;
        /*for (int x = 0; x <= 5; x++) {
			for (int y = 0; y <= 5; y++) {
				MatchPiece (x, y, 0);
				countc = 0;
			}
		}
		deletePiece ();*/
    }

    //Instantiate(ball,

    /*GameObject findBallObject(Vector3 pos)
	{
		int theX = (int)((pos.x - 80) / 100);
		int theY = (int)((pos.y - 140) / 100);
		for (int i = 0; i < Canvas.transform.childCount; i++) {
			Transform childTransform = Canvas.transform.GetChild (i);
			GameObject childObj = childTransform.gameObject;
			if (childObj.name.StartsWith ("ball")) {
				int x = (int)((childTransform.position.x - 134) / 100);
				int y = (int)((childTransform.position.y - 192) / 100);
				if (x == theX && y == theY) {
					return childObj;
				}
			}
		}
		return null;
	}*/

    // Update is called once per frame

    #region アップデート部分

    void Update()
    {
        //Score.text = score.ToString("D6");
        //if (Input.GetMouseButtonDown(0)) {
        //	Vector3 mousePos = Input.mousePosition;
        //	GameObject ballObj = findBallObject (mousePos);
        //	Ball ball = ballObj.GetComponent<Ball> ();
        //	ball.DestroyBall ();
        //}
        //return;

        switch (currentState)
        {
            case GameState.Changecolor:
                //Debug.Log("ChangeColor");
                //if(changecount <= 0){
                //    t2 = false;
                //}
                startcheck = false;
                Chain.text = chaincount.ToString() + "れんさ";
                Changecolor();
                break;
            case GameState.MatchCheck:
                //Debug.Log("MatchCheck");
                MatchCheck();
                break;
            case GameState.DeletePiece:
                //Debug.Log("DeletePiece");
                if (onetime == true)
                {
                    onetime = false;
                    DeletePiece();
                }
                break;
            case GameState.FillPiece:
                //Debug.Log("FillPiece");
                if (onetime == true)
                {
                    time += Time.deltaTime;
                    if (time >= 0.1f)
                    {
                        onetime = false;
                        FillPiece();

                    }
                }
                break;
            default:
                break;
        }
    }

    #endregion

    #region Changecolorステート

    private void Changecolor()
    {

        if (changecount <= 0)
        {
            //Endgame = true;
            Invoke("Roadscene", 0.5f);
        }
        Itemcheck = false;
        chaincount = 0;
        t2 = true;
        nc = 0;
        if (Endgame == true && changecount <= 0)
        {
            t2 = false;
            //Invoke("Roadscene", 0.5f);
            tt += Time.deltaTime;
            if (tt >= 0.8f)
            {
                SceneManager.LoadScene("Result");
            }

        }else{
            Endgame = false;
        }

        /*Touch _touch = Input.GetTouch (0); 		Vector2 worldPoint = Camera.main.ScreenToWorldPoint (_touch.position);  		if (_touch.phase == TouchPhase.Began) {  			RaycastHit2D hit = Physics2D.Raycast (worldPoint,Vector2.zero);  			if (hit) { 				 			} 		} 		if (_touch.phase == TouchPhase.Canceled) { 			 		} */

        if (t1 == true)
        {
            //Debug.Log("koreiru?");
            //SK.Skill_Plus(1);                           //タップした時にスキルカウントがプラス１される
            Change.text = changecount.ToString("D2");
            t2 = false;
            t1 = false;
            if (countc >= 7)
            {
                currentState = GameState.DeletePiece;
            }
        }
    }

    #endregion

    public void Roadscene()
    {
        //SceneManager.LoadScene("Result");
        Endgame = true;
    }


    #region MatchCheckステート

    private void MatchCheck()
    {
        //chainkeep = true;
        countc = 0;
        scorekeep = true;
        //score += nc * chaincount;
        //Itemcheck = false;
        for (int x = 0; x <= 5; x++)
        {
            for (int y = 0; y <= 5; y++)
            {
                //Debug.Log("kore");
                MatchPiece(x, y, 0);
                if (countc >= 7)
                {
                    matchcount++;
                }
                countc = 0;
            }
        }
        for (int x = 0; x <= 5; x++)
        {
            for (int y = 0; y <= 5; y++)
            {
                ItemTestDelete(x, y);
            }
        }
        //Debug.Log("matti");
        if (matchcount > 0)
        {
            currentState = GameState.DeletePiece;
            chainkeep = true;
            matchcount = 0;
        }
        else
        {
            currentState = GameState.FillPiece;
            chainkeep = true;
            matchcount = 0;
        }
    }

    #endregion

    #region DeletePieceステート

    private void DeletePiece()
    {
        t2 = false;
        for (int x = 0; x <= 5; x++)
        {
            for (int y = 0; y <= 5; y++)
            {
                ItemTestDelete(x, y);
            }
        }
        deletePiece();                              //こいつも飛んだ先で時間遅らせ 要調整
                                                    // score += nc * chaincount;
                                                    //Invoke("addScore",0.5f);
        Invoke("Fallball", 0.7f);
        //bd.Fallball();
        tm += Time.deltaTime;
        onetime = true;
        if (tm >= 0.8f)
        {                               //ここ時間遅らせ　要調整
            tm = 0;
            currentState = GameState.MatchCheck;
        }
        //Debug.Log("delete");
        if (chainkeep == true)
        {
            chainkeep = false;
            chaincount++;
            //Debug.Log(chaincount);
            SK.Skill_Plus(chaincount);
        }
    }

    #endregion

    #region FillPieceステート

    private void FillPiece()
    {
        int nl = 0;
        //chainkeep = true;
        //if (time >= 1.0f)
        //{                           //ここ時間遅らせ 要調整
            nl = FillBall();
            time = 0;
            if (nl >= 1)
            {
                currentState = GameState.MatchCheck;
                onetime = true;
                time = 0;
            }
            else if (nl == 0)
            {
                currentState = GameState.Changecolor;
                onetime = true;
                time = 0;
            }
        //}
    }

    #endregion




    //ここまでゲームの流れ　割とGameManager


    #region Changecolorステート実働部

    public void ChangeColor(int x, int y)
    {
        if (kindinfo[x, y] == changekind)
        {
            return;
        }
        SM.SendMessage("touchSE");
        SK.Skill_Plus(1);
        changecount--;
        //if (changecount <= 0){
        //    Endgame = true;
        //}
        countc = 0;
        board[x, y].GetComponent<Image>().sprite = texture[changekind];
        kindinfo[x, y] = changekind;
        changekind = next_change_kind;
        next_change_kind = Random.Range(0, kinder);
        next.GetComponent<Image>().sprite = texture[changekind];
        nextnext.GetComponent<Image>().sprite = texture[next_change_kind];
        MatchPiece(x, y, 0);
        //if(countc >= 9){
        //    //countc = 0;
        //    currentState = GameState.DeletePiece;
        //}
        //countc = 0;
    }

    #endregion

    #region ピース作るとこ

    void Createpiece(int x, int y)
    {
        board[x, y] = (Image)Instantiate(ball);
        board[x, y].transform.SetParent(Canvas.transform);
        board[x, y].rectTransform.localPosition = new Vector3(((x - 2.5f) * posX), ((5 - y) * posY));
        board[x, y].rectTransform.localScale = new Vector3(1, 1, 1);
        int kind = Random.Range(0, kinder);
        board[x, y].name = "ball [" + x + ", " + y + "]";
        board[x, y].GetComponent<Image>().sprite = texture[kind];
        board[x, y].GetComponent<Ball>().Setposition(x, y);
        kindinfo[x, y] = kind;
    }

    #endregion

    #region MatchCheckステート実働部

    //int test = 1;
    public void MatchPiece(int x, int y, int count)
    {
        if (board[x, y] == null || kindinfo[x, y] >= 5)
        {
            return;
        }
        count++;
        checkflag[x, y] = true;
        //countc += count;

        if ((x + 1 < 6) && (checkflag[x + 1, y] == false) && (deleteflag[x + 1, y] == false) && (kindinfo[x, y] == kindinfo[x + 1, y]))
        {
            MatchPiece(x + 1, y, count);
        }
        if (y + 1 < 6 && checkflag[x, y + 1] == false && deleteflag[x, y + 1] == false && kindinfo[x, y] == kindinfo[x, y + 1])
        {
            MatchPiece(x, y + 1, count);
        }
        if (x - 1 >= 0 && checkflag[x - 1, y] == false && deleteflag[x - 1, y] == false && kindinfo[x, y] == kindinfo[x - 1, y])
        {
            MatchPiece(x - 1, y, count);
        }
        if (y - 1 >= 0 && checkflag[x, y - 1] == false && deleteflag[x, y - 1] == false && kindinfo[x, y] == kindinfo[x, y - 1])
        {
            MatchPiece(x, y - 1, count);
        }

        //nc++;
        //Debug.Log(test);
        //test += 1;

        countc += count;

        checkflag[x, y] = false;

        if (countc >= 7)
        {
            statechange = true;
            flagdelete(x, y);
        }
    }

    void flagdelete(int x, int y)
    {
        deleteflag[x, y] = true;

        if (x + 1 < 6 && deleteflag[x + 1, y] == false && kindinfo[x, y] == kindinfo[x + 1, y])
        {
            flagdelete(x + 1, y);
        }
        if (y + 1 < 6 && deleteflag[x, y + 1] == false && kindinfo[x, y] == kindinfo[x, y + 1])
        {
            flagdelete(x, y + 1);
        }
        if (x - 1 >= 0 && deleteflag[x - 1, y] == false && kindinfo[x, y] == kindinfo[x - 1, y])
        {
            flagdelete(x - 1, y);
        }
        if (y - 1 >= 0 && deleteflag[x, y - 1] == false && kindinfo[x, y] == kindinfo[x, y - 1])
        {
            flagdelete(x, y - 1);
        }
    }

    #endregion

    #region DeletePieceステート実働部

    public void Fallball()
    {
        if (oneturnncmax < nc)
        {
            oneturnncmax = nc;
        }
        nc = 0;
        for (int x = 5; x >= 0; x--)
        {
            for (int y = 4; y >= 0; y--)
            {
                if (board[x, y + 1] == null)
                {
                    if (board[x, y] != null)
                    {
                        int X = x, Y = y;
                        do
                        {
                            board[X, Y + 1] = board[X, Y];
                            kindinfo[X, Y + 1] = kindinfo[X, Y];
                            board[X, Y] = null;
                            kindinfo[X, Y] = 100;
                            Y = Mathf.Clamp(Y + 1, 0, 4);
                            if (board[X, Y + 1] == null)
                            {
                                nullcontinu = true;
                            }
                            else
                            {
                                nullcontinu = false;
                            }

                        } while (nullcontinu == true);
                    }
                }
            }
        }
        //chainkeep = true;
        for (int x = 0; x <= 5; x++)
        {
            for (int y = 0; y <= 5; y++)
            {
                if (board[x, y] != null)
                {
                    board[x, y].name = "ball [" + x + ", " + y + "]";
                    //board [x, y].rectTransform.localPosition = new Vector3 (((x - 2.5f) * posX), ((5 - y) * posY));
                    board[x, y].GetComponent<Ball>().Setposition(x, y);
                }
            }
        }
    }
    //int tempx;
    //int tempy;

    public void deletePiece()
    {
        for (int x = 0; x <= 5; x++)
        {
            for (int y = 0; y <= 5; y++)
            {
                if (deleteflag[x, y] == true)
                {
                    //kindinfo[x, y] = kinder + 100;
                    kindinfo[x, y] += 100;
                    //tempx = x;
                    //tempy = y;
                    board[x, y].GetComponent<Ball>().DestroyBall();
                    //kindinfo[x, y] = kinder + 100;
                    Invoke("LateSE", 0.3f);
                    Invoke("Latedestroy", 0.7f);
                    deleteflag[x, y] = false;
                    //chainkeep = true;
                }
            }
        }
    }

    #endregion

    public void LateSE(){
        if(startcheck == true){
            return;
        }
        SM.SendMessage("deleteSE");
    }

    private void Latedestroy()
    {
        //board[tempx, tempy].GetComponent<Ball>().DestroyBall();
        Chain.text = chaincount.ToString() + "れんさ";
        Score.text = score.ToString("D6");
    }

    //public void addScore()
    //{
    //    //bool scorekeep = true;
    //    Debug.Log("来てはいるものの");
    //    Debug.Log("nc = " + nc + "chaincount = " + chaincount);
    //    if (scorekeep == true)
    //    {
    //        scorekeep = false;
    //        score += nc * chaincount;
    //    }
    //}

    #region FillPieceステート実働部

    public int FillBall()
    {
        int nullcount = 0;
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                if ((chaincount >= 4 /*|| oneturnncmax >= 7)*/ && Itemcheck == false) && startcheck == false)
                {
                    CreateItem();
                }
                if (board[x, y] == null)
                {
                    nullcount++;
                    Createpiece(x, y);
                    oneturnncmax = 0;
                }
            }
        }
        //Debug.Log (nullcount + "koko");
        //nc = nullcount;
        return nullcount;
    }

    public void CreateItem()
    {
        Itemcheck = true;
        int x = 0, y = 0;
        // Debug.Log("うわああ");
        do
        {
            x = Random.Range(0, 5);
            y = Random.Range(0, 5);
        } while (board[x, y] != null);
        //if(board[x, y] != null){
        //    return;
        //}
        board[x, y] = (Image)Instantiate(ball);
        board[x, y].transform.SetParent(Canvas.transform);
        board[x, y].rectTransform.localPosition = new Vector3(((x - 2.5f) * posX), ((5 - y) * posY));
        board[x, y].rectTransform.localScale = new Vector3(1, 1, 1);
        Itemkind = Random.Range(5, Itemmax);
        board[x, y].name = "ball [" + x + ", " + y + "]";
        board[x, y].GetComponent<Image>().sprite = texture[Itemkind];
        board[x, y].GetComponent<Ball>().Setposition(x, y);
        kindinfo[x, y] = Itemkind;
        // Debug.Log("おわ");
    }

    #endregion

    #region アイテムの動作部分

    public void ItemTestDelete(int x, int y)
    {
        if (kindinfo[x, y] < 5 || kindinfo[x, y] >= Itemmax)
        {
            return;
        }

        if (deleteflag[Mathf.Clamp(x + 1, 0, 5), y] == true || deleteflag[x, Mathf.Clamp(y + 1, 0, 5)] == true || deleteflag[Mathf.Clamp(x - 1, 0, 5), y] == true || deleteflag[x, Mathf.Clamp(y - 1, 0, 5)] == true)
        {
            switch (kindinfo[x, y])
            {
                case 5:
                    for (y = 0; y <= 5; y++)
                    {
                        if (board[x, y] != null)
                        {
                            deleteflag[x, y] = true;
                        }
                    }
                    break;
                case 6:
                    for (x = 0; x <= 5; x++)
                    {
                        if (board[x, y] != null)
                        {
                            deleteflag[x, y] = true;
                        }
                    }
                    break;
                case 8:
                    deleteflag[x, y] = true;
                    if (x + 1 <= 5) deleteflag[x + 1, y] = true;
                    if (y + 1 <= 5) deleteflag[x, y + 1] = true;
                    if (x - 1 >= 0) deleteflag[x - 1, y] = true;
                    if (y - 1 >= 0) deleteflag[x, y - 1] = true;
                    if (x + 1 <= 5 && y + 1 <= 5) deleteflag[x + 1, y + 1] = true;
                    if (x - 1 >= 0 && y + 1 <= 5) deleteflag[x - 1, y + 1] = true;
                    if (x + 1 <= 5 && y - 1 >= 0) deleteflag[x + 1, y - 1] = true;
                    if (x - 1 >= 0 && y - 1 >= 0) deleteflag[x - 1, y - 1] = true;
                    break;
                case 7:
                    deleteflag[x, y] = true;
                    changecount += 1;
                    Debug.Log("ChangeCount = " + changecount);
                    Change.text = changecount.ToString("D2");
                    break;
                default:
                    break;
            }
        }
    }

    #endregion
}
