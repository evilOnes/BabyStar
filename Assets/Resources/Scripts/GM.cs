 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class GM : MonoBehaviour {

    bool finished = true;


    GameObject 
        curPanel,
        createdObj;
    string[] Cols = { "red", "white", "yellow", "green", "black", "blue", "brown", "rainbow", "shade" };//all colors
    string[] ColsR = { "Красный", "Белый", "Желтый", "Зеленый", "Черный", "Голубой", "Коричневый", "Радуга", "Оттенки" };//all colors in russian
    string[] Figs = { "square", "ball", "rectangle", "triangle"};//all figures
    string[] FigsR = { "Квадрат", "Круг", "Прямоугольник", "Треугольник"};//all figures in russian
    string s = null;//selected string
    int idCol = 0;//selected color id
    int idFig = 0;//selected figure id
    string gameMode;//"gmtch", "yn", "shades", "pop"


    public GameObject
        mMenuPanel,
        colorsPanel,
        figuresPanel,
        digitsPanel,
        rainbowPanel,
        shadesPanel,
        choosedPanel_review,//1 step
        gameMatchPanel,//2 step
        gameYNPanel,//3 step
        gamePopPanel,//4 step
        congratsPanel;//end


    //game match stuff
    public GameObject[] matchGrid;//0 - 2x1, 1 - 2x2, 2 - 2x3
    Sprite[] matchCol;//colors
    Sprite[] matchDgt;//digits
    Sprite[] matchFig;//figures
    public static class gmtch//game match
    {
        public static string mode;//color/figures/digits
        public static int level = 0;
        public static int qtyOfRightObjs = 0; 
    };

    //rainbow stuff
    string rainbow_mode = "color";
    string[] rainbow_colors = { "red", "orange", "yellow", "green", "lightblue", "blue", "purple" };
    string[] rainbow_colorsR = { "Красный", "Оранжевый", "Желтый", "Зеленый", "Голубой", "Синий", "Фиолетовый" };
    string rainbow_pressedColor;
    int rainbow_pressedID;
    string[] rainbow_counts = { "Каждый", "Охотник", "Желает", "Знать", "Где", "Сидит", "Фазан" };
    string rainbow_pressedCount;

    //shades stuff
    string[] shades_mode = { "bright", "dark" };
    string shades_selectedMode;
    int shades_selectedModeID;
    string[] shades_modeR = { "светлее", "темнее" };
    int shades_pressedID;
    string shades_pressedShade;


    //gamePop stuff
    public static class pop
    {
        public static Vector2[] spMin = {
            new Vector2(0.04475421f, -0.5731661f),
            new Vector2(0.2526243f, -0.5697758f),
            new Vector2(0.416632f, -0.5968984f),
             new Vector2(0.563476f, -0.5935081f),
                new Vector2(0.7217625f, -0.6002888f)
        };
        public static Vector2[] spMax= {
            new Vector2(0.2755529f, -0.01592875f),
            new Vector2(0.4834231f, -0.01253847f),
            new Vector2(0.6474307f, -0.03966114f),
            new Vector2(0.7942747f, -0.03627081f),
            new Vector2(0.9525613f, -0.04305151f)
        };
        public static int spID;//spawn pos id

        public static float spawnDelay;
        public static int qtyOfBalls;
        public static bool done;

        public static bool rightBall;
    };

    //sound stuff
    AudioSource a;
    AudioClip[] clip;
    List<AudioClip> clipGood = new List<AudioClip>();
    List<AudioClip> clipBad = new List<AudioClip>();

    //score & DB stuff
    string criteria;
    int score;//score connected to database
    int scoreMax;//maxScore that we can get on this level

    // Use this for initialization
    void Start ()
    {
        score = 0;

        gmtch.qtyOfRightObjs = 0;
        matchCol = LoadSprites("Images/Objects");
        matchDgt = LoadSprites("Images/Other/DigitsObjs");
        matchFig = LoadSprites("Images/Other/Figures/FiguresObjs");
        Debug.Log("ColorObjs in total: " + matchCol.Length);
        Debug.Log("DigitObjs in total: " + matchDgt.Length);
        Debug.Log("FiguresObjs in total: " + matchFig.Length);


        AudioSource a = gameObject.GetComponent<AudioSource>();

        clip = Resources.LoadAll<AudioClip>("Sound");
        Debug.Log("Clips loaded: " + clip.Length);

        //set lists of good&bad clips
        for (int i = 0; i < clip.Length; i++)
        {
            if (clip[i].name.Contains("good"))
                clipGood.Add(clip[i]);
            else if (clip[i].name.Contains("bad"))
                clipBad.Add(clip[i]);
        }

        //hide all
        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
            if ((obj.tag == "pnl") && obj.activeInHierarchy)//8 layer = "screen" layer
                obj.SetActive(false);

        curPanel = mMenuPanel;
        mMenuPanel.SetActive(true);

    }

  
    public void SetGameMatchPanel()
    {
        gameMode = "gmtch";

        //set hint button action
        GameObject.Find("btnHint").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("btnHint").GetComponent<Button>().onClick.AddListener(
            delegate () { StartCoroutine(ShowHint()); });

        Sprite[] matches = new Sprite[1];

        //set matches resources here: is it colors or is it digits
        if (gmtch.mode == "colors")
            matches = matchCol;
        else if (gmtch.mode == "digits")
            matches = matchDgt;
        else if (gmtch.mode == "figures")
            matches = matchFig;

        Debug.Log("\tLevel: " + gmtch.level + "\tArray.Length: " + matchGrid.Length);
        if(gmtch.level == matchGrid.Length)
        {
            //stop increment & goto congrats
            gmtch.level = 0;
            Show(congratsPanel);

            //send score to database
            DB.SetScore(criteria, score, scoreMax);
            scoreMax = 0;

            //set congrats panel
            try
            {
                Destroy(GameObject.Find("btnDone"));
            }
            catch (System.Exception e) { }

            GameObject btnNext = CreateFromPrefab("Prefabs/btnNext", congratsPanel);
            btnNext.name = "btnNext";
            btnNext.GetComponent<Button>().onClick.RemoveAllListeners();
            btnNext.GetComponent<Button>().onClick.AddListener(
                delegate()
                {
                    Show(gameYNPanel);
                    SetGameYNPanel();
                    
                });
            GameObject.Find("btnBack").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("btnBack").GetComponent<Button>().onClick.AddListener(
                delegate ()
                {
                    //show selected mode
                    if (gmtch.mode == "colors")
                        Show(colorsPanel);
                    else if (gmtch.mode == "digits")
                        Show(digitsPanel);
                    else if (gmtch.mode == "figures")
                        Show(figuresPanel);
                });
            return;
        }

        Button bBack = GameObject.Find("btnBack").GetComponent<Button>();
        bBack.onClick.RemoveAllListeners();
        bBack.onClick.AddListener(
            delegate ()
            {
                Show(choosedPanel_review);



                //if (gmtch.mode == "colors")
                //    SetColor();
                //else if (gmtch.mode == "digits")
                //    SetDigit();
                //else if (gmtch.mode == "figures")
                //    SetFigure();

                //reset game match level
                gmtch.level = 0;
            });
        
        GameObject grid;

        RemoveGrids();

        grid = Instantiate(matchGrid[gmtch.level], gameMatchPanel.transform.localPosition, Quaternion.identity) as GameObject;
        grid.tag = "grid";
        grid.transform.SetParent(gameMatchPanel.transform, false);//this to display item correctly
        grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        

        Sprite tmpSprite;
        bool foundCorrect = false;
        Transform tmpChild;
        ColorBlock cb;
        int breaker = 0;
        do
        {
            //this is to prevent looping
            if (breaker++ > 100)
            {
                Debug.Log("\tEntered continious loop!");
                break;
            }

            //reset qtyOfRightObjs 'cos if we won't - it'll count already removed sprites too
            gmtch.qtyOfRightObjs = 0;
            for (int i = 0; i < grid.transform.childCount; i++)
            {
                tmpSprite = matches[Random.Range(0, matches.Length - 1)];//select random sprite

                tmpChild = grid.transform.GetChild(i);
                tmpChild.name = tmpSprite.name;
                tmpChild.GetComponent<Image>().sprite = tmpSprite;

                //set highlight color & increment qtyOfRightObjs
                cb = tmpChild.GetComponent<Button>().colors;
                if (tmpChild.name.ToLower().Contains(s))
                {
                    cb.highlightedColor = Color.green;
                    gmtch.qtyOfRightObjs++;

                    Debug.Log("Right obj (" + tmpChild.name + ") added. Total: " + gmtch.qtyOfRightObjs + 
                        "\nSelected: " + s);
                }
                else
                    cb.highlightedColor = Color.red;
                tmpChild.GetComponent<Button>().colors = cb;

                tmpChild.GetComponent<Button>().onClick.RemoveAllListeners();
                tmpChild.GetComponent<Button>().onClick.AddListener(delegate () { CheckClickedObj(gameMatchPanel); });

                foundCorrect = tmpSprite.name.ToLower().Contains(s);
            }
        } while (!foundCorrect);

        scoreMax += gmtch.qtyOfRightObjs;
        Debug.Log("scoreMax = " + scoreMax);
    }
    public void SetGameYNPanel()
    {
        Debug.Log("YN started.");
        gameMode = "yn";

        GameObject btnHint = GameObject.Find("btnHint");
        Debug.Log("btnHint found: " + btnHint.name);
        btnHint.GetComponent<Button>().onClick.RemoveAllListeners();
        btnHint.GetComponent<Button>().onClick.AddListener(
            delegate ()
            {
                StartCoroutine(ShowHint());
            });

        if (gmtch.level == 3)
        {
            Debug.Log("YN done.");


            scoreMax = 3;
            Debug.Log("scoreMax = " + scoreMax);

            //send score to database
            DB.SetScore(criteria, score, scoreMax);

            //stop increment & goto congrats
            gmtch.level = 0;
            Show(gamePopPanel);
            SetGamePop();
            //set game pop panel
            
            GameObject.Find("btnBack").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("btnBack").GetComponent<Button>().onClick.AddListener(
                delegate ()
                {
                    //show selected mode
                    if (gmtch.mode == "colors")
                        Show(colorsPanel);
                    else if (gmtch.mode == "digits")
                        Show(digitsPanel);
                    else if (gmtch.mode == "figures")
                        Show(figuresPanel);
                });
            return;
        }

        //create grid
        GameObject grid;

        RemoveGrids();

        grid = CreateFromPrefab("Prefabs/grid1x1_YN", gameYNPanel);
        grid.tag = "grid";
        grid.transform.SetParent(gameYNPanel.transform, false);//this to display item correctly
        grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);


        Sprite[] matches = new Sprite[1];

        //set matches resources here: is it colors or is it digits
        if (gmtch.mode == "colors")
            matches = matchCol;
        else if (gmtch.mode == "digits")
            matches = matchDgt;
        else if (gmtch.mode == "figures")
            matches = matchFig;

        Sprite tmpSprite;
        bool foundCorrect = false;
        Transform tmpChild = null;
        int breaker = 0;
        do
        {
            //this is to prevent looping
            if (breaker++ > 100)
            {
                Debug.Log("\tEntered continuous loop!");
                break;
            }

            //reset qtyOfRightObjs 'cos if we won't - it'll count already removed sprites too
            gmtch.qtyOfRightObjs = 0;
               
                tmpSprite = matches[Random.Range(0, matches.Length - 1)];//select random sprite

                tmpChild = grid.transform.GetChild(0);// grid/obj

                //set sprite
                tmpChild.name = tmpSprite.name;
                tmpChild.GetComponent<Image>().sprite = tmpSprite;

                foundCorrect = tmpChild.GetComponent<Image>().sprite.name.ToLower().Contains(s);

            if (gmtch.qtyOfRightObjs > 1) foundCorrect = false;
            

            //no rule for exiting for now
        } while (true); //while (!foundCorrect);

        //check which sprite is on question side
       
        Debug.Log("Sprite name: " + tmpChild.GetComponent<Image>().sprite.name.ToLower() +
            "\nContains: " + s + 
            "\nResult: " + foundCorrect);

        GameObject bYes, bNo;
        bYes = GameObject.Find("btnYes");
        bNo = GameObject.Find("btnNo");
        if(foundCorrect)
        {
            bYes.GetComponent<Button>().onClick.RemoveAllListeners();
            bYes.GetComponent<Button>().onClick.AddListener(delegate () { YN_setYesBtn(); });

            bNo.GetComponent<Button>().onClick.RemoveAllListeners();
            bNo.GetComponent<Button>().onClick.AddListener(delegate () { YN_setNoBtn(); });
        }
        else
        {
            bYes.GetComponent<Button>().onClick.RemoveAllListeners();
            bYes.GetComponent<Button>().onClick.AddListener(delegate () { YN_setNoBtn(); });

            bNo.GetComponent<Button>().onClick.RemoveAllListeners();
            bNo.GetComponent<Button>().onClick.AddListener(delegate () { YN_setYesBtn(); });
        }


    }
    public void YN_setYesBtn()
    {        
        gmtch.qtyOfRightObjs = 0;
        gmtch.level++;
        AudioSource a = gameObject.GetComponent<AudioSource>();
        a.PlayOneShot(clipGood[Random.Range(0, clipGood.Count)]);
        SetGameYNPanel();
    }
    public void YN_setNoBtn()
    {
        StartCoroutine(CreateObjForFixedTime("Prefabs/Text", gameYNPanel, 1f));
        createdObj.GetComponent<Text>().text = "Не правильно!";
        AudioSource a = gameObject.GetComponent<AudioSource>();
        a.PlayOneShot(clipBad[Random.Range(0, clipBad.Count)]);
    }
    public void Rainbow_SetMode()
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
        string objName = clickedObj.name.ToLower();
        if (objName.Contains("color"))
            rainbow_mode = "color";
        else if (objName.Contains("count"))
            rainbow_mode = "count";
        Debug.Log("Selected mode: " + rainbow_mode);
    }
    public void Rainbow_ColorPressed()
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
        string objName = clickedObj.name.ToLower().Substring(3);
        for(int i = 0; i < rainbow_colors.Length; i++)
            if(objName == rainbow_colors[i])
            {
                rainbow_pressedID = i; 
                rainbow_pressedColor = rainbow_colorsR[i];
                rainbow_pressedCount = rainbow_counts[i];
                break;
            }
        StartCoroutine(CreateObjForFixedTime("Prefabs/Text", rainbowPanel, 1f));
        GameObject text = createdObj;
        if (rainbow_mode == "color")
            text.GetComponent<Text>().text = rainbow_pressedColor;
        else if (rainbow_mode == "count")
            text.GetComponent<Text>().text = rainbow_pressedCount;
    }
    public void SetShadesPanel()
    {
        //2x1 grid
        //only 1 right obj


        //choose mode
        shades_selectedModeID = Random.Range(0, 2);
        shades_selectedMode = shades_mode[shades_selectedModeID];
        s = shades_selectedMode;
        gameMode = "shades";

        //set hint button action
        GameObject.Find("btnHint").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("btnHint").GetComponent<Button>().onClick.AddListener(
            delegate () { StartCoroutine(ShowHint()); });

        Sprite[] matches = new Sprite[1];


        matches = LoadSprites("Images/Other/Shades");

        Debug.Log("\tgameShades Level: " + gmtch.level );
        if (gmtch.level == 3)
        {
            //stop increment & goto congrats
            gmtch.level = 0;
            Show(congratsPanel);
            //set congrats panel
            Destroy(GameObject.Find("btnNext"));
            GameObject btnDone = CreateFromPrefab("Prefabs/btnDone", congratsPanel);

            btnDone.GetComponent<Button>().onClick.RemoveAllListeners();
            btnDone.GetComponent<Button>().onClick.AddListener(
                delegate ()
                {
                    //show selected mode
                    if (gmtch.mode == "colors")
                        Show(colorsPanel);
                    else if (gmtch.mode == "digits")
                        Show(digitsPanel);
                    else if (gmtch.mode == "figures")
                        Show(figuresPanel);

                });
            GameObject.Find("btnBack").GetComponent<Button>().onClick.RemoveAllListeners();
            GameObject.Find("btnBack").GetComponent<Button>().onClick.AddListener(
                delegate ()
                {
                    //show selected mode
                    if (gmtch.mode == "colors")
                        Show(colorsPanel);
                    else if (gmtch.mode == "digits")
                        Show(digitsPanel);
                    else if (gmtch.mode == "figures")
                        Show(figuresPanel);
                });
            return;
        }


        //FillGrid
        GameObject grid;
        string gridPath;
        gridPath = "Prefabs/grid2x1";

        RemoveGrids();

        grid = Instantiate(Resources.Load(gridPath), shadesPanel.transform.localPosition, Quaternion.identity) as GameObject;
        grid.tag = "grid";
        grid.transform.SetParent(shadesPanel.transform, false);//this to display item correctly
        grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);


        Sprite tmpSprite;
        bool foundCorrect = false;
        Transform tmpChild;
        ColorBlock cb;
        int breaker = 0;
        do
        {
            //this is to prevent looping
            if (breaker++ > 100)
            {
                Debug.Log("\tEntered continious loop!");
                break;
            }

            //reset qtyOfRightObjs 'cos if we won't - it'll count already removed sprites too
            gmtch.qtyOfRightObjs = 0;
            for (int i = 0; i < 2; i++)
            {
                tmpSprite = matches[Random.Range(0, matches.Length - 1)];//select random sprite

                tmpChild = grid.transform.GetChild(i);
                tmpChild.name = tmpSprite.name;
                tmpChild.GetComponent<Image>().sprite = tmpSprite;


                //Debug.Log("\tobj (" + tmpChild.name + ") added. Total: " + gmtch.qtyOfRightObjs +
                //        "\nSelected: " + s);

                //set highlight color & increment qtyOfRightObjs
                cb = tmpChild.GetComponent<Button>().colors;
                if (tmpChild.name.ToLower().Contains(s))
                {
                    cb.highlightedColor = Color.green;
                    gmtch.qtyOfRightObjs++;
                    Debug.Log("Right obj (" + tmpChild.name + ") added. Total: " + gmtch.qtyOfRightObjs +
                        "\nSelected: " + s);
                }
                else
                    cb.highlightedColor = Color.red;
                tmpChild.GetComponent<Button>().colors = cb;

                tmpChild.GetComponent<Button>().onClick.RemoveAllListeners();
                tmpChild.GetComponent<Button>().onClick.AddListener(delegate () { CheckClickedObj(shadesPanel); });

                foundCorrect = tmpChild.name.ToLower().Contains(s);


                if (gmtch.qtyOfRightObjs > 1) foundCorrect = false;
            }
        } while (!foundCorrect);

    }
    void FillGridShadesReview(Sprite[] matches)
    {
        //choose mode
        shades_selectedModeID = 0;
        shades_selectedMode = shades_mode[shades_selectedModeID];
        s = shades_selectedMode;
        gameMode = "shades";

        Debug.Log("FillGridShades() triggered.\nS: " + s);


        RemoveGrids();

        GameObject grid;
        string gridPath;
        gridPath = "Prefabs/grid2x1_shadesReview";

        grid = Instantiate(Resources.Load(gridPath), choosedPanel_review.transform.localPosition, Quaternion.identity) as GameObject;
        grid.tag = "grid";
        grid.transform.SetParent(choosedPanel_review.transform, false);//this to display item correctly
        grid.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 165.6f);
        

        Sprite tmpSprite;
        bool foundCorrect = false;
        Transform tmpChild;
        ColorBlock cb;
        int breaker = 0;
        do
        {
            //this is to prevent looping
            if (breaker++ > 100)
            {
                Debug.Log("\tEntered continious loop!");
                break;
            }

            //reset qtyOfRightObjs 'cos if we won't - it'll count already removed sprites too
            gmtch.qtyOfRightObjs = 0;
            for (int i = 0; i < 2; i++)
            {
                tmpSprite = matches[Random.Range(0, matches.Length - 1)];//select random sprite

                tmpChild = grid.transform.GetChild(i);
                tmpChild.name = tmpSprite.name;
                tmpChild.GetComponent<Image>().sprite = tmpSprite;


                //Debug.Log("\tobj (" + tmpChild.name + ") added. Total: " + gmtch.qtyOfRightObjs +
                //        "\nSelected: " + s);

                //set highlight color & increment qtyOfRightObjs
                cb = tmpChild.GetComponent<Button>().colors;
                if (tmpChild.name.ToLower().Contains(s))
                {
                    cb.highlightedColor = Color.green;
                    gmtch.qtyOfRightObjs++;
                    Debug.Log("Right obj (" + tmpChild.name + ") added. Total: " + gmtch.qtyOfRightObjs +
                        "\nSelected: " + s);
                }
                else
                    cb.highlightedColor = Color.red;
                tmpChild.GetComponent<Button>().colors = cb;

                tmpChild.GetComponent<Button>().onClick.RemoveAllListeners();

                foundCorrect = tmpChild.name.ToLower().Contains(s);


                if (gmtch.qtyOfRightObjs > 1) foundCorrect = false;

                //reselect mode
                shades_selectedModeID = 1;
                shades_selectedMode = shades_mode[shades_selectedModeID];
                s = shades_selectedMode;
            }
        } while (!foundCorrect);
    }
    public void SetReviewPanel()
    {
        Debug.Log("SetReviewPanel() triggered.");
        Debug.Log("idCol: " + idCol);
        RunTeller();
        if (idCol > 6)
        {
            if (idCol == 7)//if its rainbow
                RemoveGrids();
            return;
        }

        //set path depending on selected mode (color & figure & digit)
        string path = null;
        Debug.Log("gmtch.mode: " + gmtch.mode);
        if (gmtch.mode == "colors")
            path = "Images/Objects";

        else if (gmtch.mode == "figures")
            path = "Images/Other/Figures/FiguresObjs";

        else if (gmtch.mode == "digits")
            path = "Images/Other/DigitsObjs";

        //load sprites
        Sprite[] examples = LoadSprites(path);

        //create grid
        RemoveGrids();
        GameObject grid = CreateFromPrefab("Prefabs/examplesGrid3x1", choosedPanel_review);

        FillGrid(ref grid, examples);
        

    }
    void FillGrid(ref GameObject grid, Sprite[] matches)
    {
        Sprite tmpSprite = null;
        Transform tmpChild;

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            int bbreaker = 0;
            do
            {
                if (bbreaker++ > 1000)
                {
                    Debug.Log("Loop in loop gone mad." +
                        "S: " + s);
                    break;
                }

                tmpSprite = matches[Random.Range(0, matches.Length)];//select random sprite

                for (int j = 0; j < grid.transform.childCount; j++)
                {
                    //to prevent duplicates
                    if (grid.transform.GetChild(j).gameObject.name == tmpSprite.name)
                        tmpSprite.name = "not";
                }

            } while (!tmpSprite.name.ToLower().Contains(s));



            tmpChild = grid.transform.GetChild(i);
            tmpChild.name = tmpSprite.name;
            tmpChild.GetComponent<Image>().sprite = tmpSprite;


            tmpChild.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
  
    public void SetGamePop()
    {
        Debug.Log("Game Pop started.");

        //set gameMode
        gameMode = "pop";
        //reset counter
        gmtch.qtyOfRightObjs = 0;

        pop.qtyOfBalls = 10;
        pop.spawnDelay = 1f;
        StartCoroutine(GamePop_SpawnBalls());

    }

    public void CheckClickedObj(GameObject panel)
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;

        //set audio
        AudioSource a = gameObject.GetComponent<AudioSource>();

        Debug.Log("Clicked obj: " + clickedObj.name + "\nSelected: " + s);
        if (clickedObj.name.ToLower().Contains(s))
        {
            StartCoroutine(DestroyObjWithDelay_gameMatch(clickedObj));
            gmtch.qtyOfRightObjs--;

            //set & play clip good
            a.PlayOneShot(clipGood[Random.Range(0, clipGood.Count)]);

            //add point to score
            score++;
        }
        else
        {
            //set & play clip bad
            a.PlayOneShot(clipBad[Random.Range(0, clipBad.Count)]);
        }

        Debug.Log("Objs left: " + gmtch.qtyOfRightObjs);
        //check: if all right objs selected - goto next stage
        if (gmtch.qtyOfRightObjs == 0)
        {
            gmtch.level++;
            if (panel.name.ToLower().Contains("gamematch"))
                SetGameMatchPanel();
            else if (panel.name.ToLower().Contains("yn"))
                SetGameYNPanel();
            else if (panel.name.ToLower().Contains("shade"))
                SetShadesPanel();
            else if (panel.name.ToLower().Contains("pop"))
                SetGamePop();
        }
    }
    public void GamePop_CheckClicked()
    {
        Debug.Log("GamePop_CheckClicked() triggered.");

        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
        Debug.Log("clicked object name: " + clickedObj.name +
            "\nselected: " + s);
        if (clickedObj.name.ToLower().Contains(s))
        {
            AudioSource a = gameObject.GetComponent<AudioSource>();
            a.PlayOneShot(clipGood[Random.Range(0, clipGood.Count)]);
            clickedObj = clickedObj.transform.parent.gameObject;//to delete whole ballPop
            StartCoroutine(DestroyObjWithDelay_gameMatch(clickedObj));
            //gmtch.qtyOfRightObjs--;
        }
        else
        {
            AudioSource a = gameObject.GetComponent<AudioSource>();
            a.PlayOneShot(clipBad[Random.Range(0, clipBad.Count)]);
        }
    }
    public void RemoveGrids()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("grid"))
            Destroy(g);
    }

    //COLORS PART
    public void SetColor()
    {
        //set match mode
        gmtch.mode = "colors";

        Debug.Log("gmtch.mode at SetColor(): " + gmtch.mode);

        //set action on buttons
        Button bBack = GameObject.Find("btnBack").GetComponent<Button>();
        bBack.onClick.RemoveAllListeners();
        bBack.onClick.AddListener(
            delegate ()
            {
                Show(colorsPanel);
            }
            );

        for (int i = 0; i < Cols.Length; i++)
            if (EventSystem.current.currentSelectedGameObject.gameObject.name.ToLower().Contains(Cols[i]))
            {
                s = Cols[i];
                idCol = i;
            }

        Button bNext = GameObject.Find("btnNext").GetComponent<Button>();
        bNext.onClick.RemoveAllListeners();
        bNext.onClick.AddListener(
            delegate ()
            {
                if (idCol < 7)//cos 7, 8 not just colors
                {
                    Show(gameMatchPanel);
                    SetGameMatchPanel();
                }
                else if (idCol == 7)//rainbow
                {
                    Show(rainbowPanel);
                    SetGameYNPanel();
                }
                else if (idCol == 8)//shades
                {
                    Show(shadesPanel);
                    SetShadesPanel();
                }
            }
            );

        GameObject.Find("colorText").GetComponent<Text>().text = ColsR[idCol];

        criteria = ColsR[idCol];
    }
    public void SetEyes()//delete this + animator + show delayed + show with delay
    {
        //Debug.Log("\tSelected obj: " + EventSystem.current.currentSelectedGameObject.name);
        //EventSystem.current.currentSelectedGameObject.GetComponent<Animator>().SetBool("clicked", true);
        Sprite[] ballsE = LoadSprites("Images/Other/Balls/Eyes");
        Sprite spr = null;
        foreach (var b in ballsE)
            if (b.name.ToLower().Contains("red"))
            {
                spr = b;
                break;
            }
        EventSystem.current.currentSelectedGameObject.GetComponent<Button>().image.sprite = spr;
    }

    //DIGITS PART
    public void SetDigit()
    {
        //set action on btnBack
        Button bBack = GameObject.Find("btnBack").GetComponent<Button>();
        bBack.onClick.RemoveAllListeners();
        bBack.onClick.AddListener(
            delegate ()
            {
                Show(digitsPanel);
            }
            );

        Button bNext = GameObject.Find("btnNext").GetComponent<Button>();
        bNext.onClick.RemoveAllListeners();
        bNext.onClick.AddListener(
            delegate ()
            {
                Show(gameMatchPanel);
                SetGameMatchPanel();
            }
            );

        s = EventSystem.current.currentSelectedGameObject.gameObject.name.Substring(3, 1);

        //set match mode
        gmtch.mode = "digits";

        GameObject.Find("colorText").GetComponent<Text>().text = s;
        criteria = s;
    }

    //FIGURES PART
    public void SetFigure()
    {
        //set act on btnBack
        Button bBack = GameObject.Find("btnBack").GetComponent<Button>();
        bBack.onClick.RemoveAllListeners();
        bBack.onClick.AddListener(
            delegate ()
            {
                Show(figuresPanel);
            });

        Button bNext = GameObject.Find("btnNext").GetComponent<Button>();
        bNext.onClick.RemoveAllListeners();
        bNext.onClick.AddListener(
            delegate()
            {
                Show(gameMatchPanel);
                SetGameMatchPanel();
            });

        //get name of figure
        string tmps = EventSystem.current.currentSelectedGameObject.name;
        tmps = tmps.Split(' ')[1].ToLower();
        for(int i = 0; i < 10; i++)
            if(tmps.Contains(Figs[i]))
            {
                idFig = i;
                break;
            }
        else if(Figs[i].Contains(tmps))
            {
                idFig = i;
                break;
            }

        s = Figs[idFig];

        //set match mode
        gmtch.mode = "figures";

        GameObject.Find("colorText").GetComponent<Text>().text = FigsR[idFig];

        criteria = FigsR[idFig];
    }
    
    public void Show(GameObject obj)
    {
        if (obj.name == "mainMenu")
            SceneManager.LoadScene(0);

        if (obj != null)
        {
            curPanel.SetActive(false);
            obj.SetActive(true);
            curPanel = obj;
        }
    }
    public void ShowPlus(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(true);
    }
    public void Hide(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(false);
    }
    public void ShowDelayed(GameObject obj)
    {
        StartCoroutine(ShowWithDelay(obj));
    }
    public IEnumerator ShowWithDelay(GameObject obj)
    {
        yield return new WaitForSeconds(1f);
        if (obj != null)
        {
            curPanel.SetActive(false);
            obj.SetActive(true);
            curPanel = obj;
        }
    }

    //LOCAL
    public void SetSound(GameObject obj = null)
    {
        if (obj == null)
            obj = EventSystem.current.currentSelectedGameObject;

        string strToFind = null;
        Debug.Log("GET TYPE name of clicked object: " + obj.name);
        string type = obj.name;
        type = type.Remove(0, 3);
        type = type.ToLower();
        Debug.Log("GET TYPE name of clicked object: " + obj.name);

        //for hint
        string gMode = gameMode;
        if (gMode == "gmtch")
            gMode = "gm";

        if (type == "hint")
            strToFind = type + "_" + gMode;

        //for teller
        else if (type == "teller")
            strToFind = type + "_" + s;

        //for mMenu
        if (type == "colors")
            strToFind = "choose_color";
        else if (type == "figures")
            strToFind = "choose_figure";
        else if (type == "digits")
            strToFind = "choose_digit";

        //set sound
        AudioSource a = gameObject.GetComponent<AudioSource>();
        a.Stop();

        Debug.Log("Starting \"SELECT CLIP\"");

        //select audio clip
        AudioClip selClip = null;
        for (int i = 0; i < clip.Length; i++)
            if (clip[i].name.ToLower().Contains(strToFind))
            {
                selClip = clip[i];
                break;
            }
        Debug.Log("AudioClip: " + selClip.name);

        a.PlayOneShot(selClip);
    }
    public void SetSound()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;

        string strToFind = null;
        Debug.Log("GET TYPE name of clicked object: " + obj.name);
        string type = obj.name;
        type = type.Remove(0, 3);
        type = type.ToLower();
        Debug.Log("GET TYPE name of clicked object: " + obj.name);

        //for hint
        string gMode = gameMode;
        if (gMode == "gmtch")
            gMode = "gm";

        if (type == "hint")
        {
            strToFind = type + "_" + gMode;
            strToFind += "_" + s;
        }

        //for teller
        else if (type == "teller")
            strToFind = type + "_" + s;

        //for mMenu
        if (type == "colors")
            strToFind = "choose_color";
        else if (type == "figures")
            strToFind = "choose_figure";
        else if (type == "digits")
            strToFind = "choose_digit";

        //final string to find
        Debug.Log("Final string to find: " + strToFind);

        //set sound
        AudioSource a = gameObject.GetComponent<AudioSource>();
        a.Stop();

        Debug.Log("Starting \"SELECT CLIP\"");

        //select audio clip
        AudioClip selClip = null;
        for (int i = 0; i < clip.Length; i++)
            if (clip[i].name.ToLower().Contains(strToFind))
            {
                selClip = clip[i];
                break;
            }
        Debug.Log("AudioClip: " + selClip.name);

        a.PlayOneShot(selClip);
    }
    public void ShowHintInst()
    {
        Debug.Log("ShowHintInst() triggered.");
        StartCoroutine(ShowHint());
    }
    public IEnumerator ShowHint()
    {
        Debug.Log("IEnum ShowHint() triggered.");
        Debug.Log("game mode: " + gameMode);
        GameObject 
            hint,
            parent = null;

        if (gameMode == "yn")
            parent = gameYNPanel;
        else if (gameMode == "gmtch")
            parent = gameMatchPanel;
        else if (gameMode == "shades")
            parent = shadesPanel;
        else if (gameMode == "pop")
            parent = gamePopPanel;

        hint = CreateFromPrefab("Prefabs/Hint", parent);

        hint.transform.SetParent(parent.transform, false);//this to display item correctly
        hint.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        string hintText = s;
        if (gmtch.mode == "colors")//then it's colors
            hintText = ColsR[idCol];

        else if (gmtch.mode == "figures")
            hintText = FigsR[idFig];


        if (gameMode == "yn")
            hintText = "Это " + hintText + "?";
        else if (gameMode == "shades")
            hintText = "Выбери " + shades_modeR[shades_selectedModeID];
        else if (gameMode == "pop")
            hintText = "Лопай шары с предметами похожие на " + hintText;

        hint.transform.GetChild(0).GetComponent<Text>().text = hintText;

        yield return new WaitForSeconds(1f);
        Destroy(hint);
    }
    IEnumerator GamePop_SpawnBalls()
    {
        pop.rightBall = false;
        int breaker = 0;
        do {
            if(breaker++ > 1000)
            {
                Debug.Log("Continious loop! at GamePop_SpawnBalls()");
                break;
            }
            for (int i = 0; i < pop.qtyOfBalls; i++)
            {
                //create ballPop
                GameObject ballPop = CreateFromPrefab("Prefabs/ballPop", gamePopPanel);

                //set tag
                ballPop.tag = "ballPop";

                //set position
                pop.spID = Random.Range(0, pop.spMin.Length);
                ballPop.GetComponent<RectTransform>().anchorMin = pop.spMin[pop.spID];
                ballPop.GetComponent<RectTransform>().anchorMax = pop.spMax[pop.spID];
                ballPop.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

                //load sprites
                Sprite[] matches = null;
                if (gmtch.mode == "colors")
                    matches = matchCol;
                else if (gmtch.mode == "figures")
                    matches = matchFig;
                else if (gmtch.mode == "digits")
                    matches = matchDgt;

                //get child where we will set sprite
                GameObject objHolder = ballPop.transform.GetChild(0).gameObject;
                GameObject objBckgnd = ballPop.transform.GetChild(1).gameObject;

                //set sprite
                objHolder.GetComponent<Button>().image.sprite = matches[Random.Range(0, matches.Length)];
                //set name of first sprite to objHolder
                objHolder.name = objHolder.GetComponent<Button>().image.sprite.name;
                objBckgnd.name = objHolder.name;

                //set onclick
                objBckgnd.GetComponent<Button>().onClick.RemoveAllListeners();
                objBckgnd.GetComponent<Button>().onClick.AddListener(delegate () { GamePop_CheckClicked(); });

                ColorBlock cb = objBckgnd.GetComponent<Button>().colors;

                if (objHolder.name.ToLower().Contains(s))
                {
                    //set green highlight color
                    cb.highlightedColor = Color.green;
                    objBckgnd.GetComponent<Button>().colors = cb;
                    pop.rightBall = true;

                    //add +1 to qtyOfRightObjs if it's right obj
                    gmtch.qtyOfRightObjs++;
                }
                else
                {
                    //set red highlight color
                    cb.highlightedColor = Color.red;
                    objBckgnd.GetComponent<Button>().colors = cb;
                }




                yield return new WaitForSeconds(pop.spawnDelay);
            }
        } while (!pop.rightBall);

        //end of game pop.done controlled from ballPop.cs
        yield return new WaitUntil(() => pop.done);

        foreach (var obj in GameObject.FindGameObjectsWithTag("ballPop"))
            Destroy(obj);

        //stop increment & goto congrats
        gmtch.level = 0;
        Show(congratsPanel);
        //set congrats panel
        Destroy(GameObject.Find("btnNext"));
        GameObject btnDone = CreateFromPrefab("Prefabs/btnDone", congratsPanel);

        btnDone.GetComponent<Button>().onClick.RemoveAllListeners();
        btnDone.GetComponent<Button>().onClick.AddListener(
            delegate ()
            {
                    //show selected mode
                    if (gmtch.mode == "colors")
                    Show(colorsPanel);
                else if (gmtch.mode == "digits")
                    Show(digitsPanel);
                else if (gmtch.mode == "figures")
                    Show(figuresPanel);

            });
        GameObject.Find("btnBack").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("btnBack").GetComponent<Button>().onClick.AddListener(
            delegate ()
            {
                    //show selected mode
                    if (gmtch.mode == "colors")
                    Show(colorsPanel);
                else if (gmtch.mode == "digits")
                    Show(digitsPanel);
                else if (gmtch.mode == "figures")
                    Show(figuresPanel);
            });

    }
    public void RunTeller()
    {
        SetSound(GameObject.Find("btnTeller"));

        if (idCol == 8)
        {
            StartCoroutine(Teller_shades());
        }
        else
            StartCoroutine(Teller());
    }
    IEnumerator Teller()
    {
        Debug.Log("Teller has started");

        GameObject.Find("btnTeller").GetComponent<Animator>().SetBool("tellerDone", false);

        float storyTime, showTime;

        if (gmtch.mode.Contains("color"))
        {
            storyTime = 7.5f;
            showTime = 5f;
        }
        else if (gmtch.mode.Contains("digit"))
        {
            storyTime = 4f;
            showTime = 4f;
        }
        else
        {
            storyTime = 2f;
            showTime = 7f;
        }

        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandWave", choosedPanel_review, storyTime));//create handWave
        yield return new WaitForSeconds(storyTime);
        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandPoint_examples", choosedPanel_review, 3f));//create handPoint_examples
        yield return new WaitForSeconds(showTime);
        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandPoint_btnNext", choosedPanel_review, 2f));//create handPoint_btnNext

        GameObject.Find("btnTeller").GetComponent<Animator>().SetBool("tellerDone", true);
    }
    IEnumerator Teller_shades()
    {
        Debug.Log("Teller for shades Started.");

        FillGridShadesReview(LoadSprites("Images/Other/Shades"));

        GameObject.Find("btnTeller").GetComponent<Animator>().SetBool("tellerDone", false);

        float storyTime = 5.5f;
        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandWave", choosedPanel_review, storyTime));//create handWave
        yield return new WaitForSeconds(storyTime);
        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandPoint_shadeLeft", choosedPanel_review, 2f));//create handPoint_examples
        yield return new WaitForSeconds(2f);
        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandPoint_shadeRight", choosedPanel_review, 2f));//create handPoint_examples
        yield return new WaitForSeconds(4f);
        StartCoroutine(CreateObjForFixedTime("Prefabs/imgHandPoint_btnNext", choosedPanel_review, 2f));//create handPoint_btnNext

        GameObject.Find("btnTeller").GetComponent<Animator>().SetBool("tellerDone", true);
    }
    IEnumerator CreateObjForFixedTime(string path, GameObject panel, float time)
    {
        finished = false;
        GameObject obj = CreateFromPrefab(path, panel);
        createdObj = obj;
        Debug.Log("Created obj: " + obj.name);
        yield return new WaitForSeconds(time);
        Destroy(obj);
        Debug.Log("Created obj (" + obj.name + ") has been destroyed");
        finished = true;

    }
    Sprite[] LoadSprites(string path)
    {
        Debug.Log("Loading sprites at path: " + path);
        return Resources.LoadAll<Sprite>(path);
    }
    IEnumerator DestroyObjWithDelay_gameMatch(GameObject obj, float timeDelay = 0.3f, bool destroy = true)
    {
        yield return new WaitForSeconds(timeDelay);
        if (destroy)
            Destroy(obj);
    }
    GameObject CreateFromPrefab(string path, GameObject panel)
    {
        GameObject obj = null;
        Debug.Log("Create from prefab path: " + path);
        obj = Instantiate(
            Resources.Load(path) as GameObject,
            panel.GetComponent<Transform>().localPosition,
            Quaternion.identity);
        obj.transform.SetParent(panel.transform, false);//this to display item correctly
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        return obj;
    }

    //GLOBAL
    public static int abs(int a)
    {
        if (a > 0)
            return a;
        else
            return a * -1;
    }
    public static float abs(float a)
    {
        if (a > 0)
            return a;
        else
            return a * -1;
    }
}
