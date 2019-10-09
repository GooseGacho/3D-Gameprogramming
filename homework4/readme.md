## 编写一个简单的鼠标打飞碟（Hit UFO）游戏
本文利用MVC模式以及工厂模式以及鼠标与游戏的交互实现打飞碟游戏。
游戏演示视频地址： http://www.iqiyi.com/w_19ryayfomt.html
<html><hr></html>

 - 游戏内容要求：
 	- 游戏有 n 个 round，每个 round 都包括10 次 trial；
	 - 每个 trial 的飞碟的色彩、大小、发射位置、速度、角度、同时出现的个数都可能不同。它们由该 round 的 ruler 控制；
	 - 每个 trial 的飞碟有随机性，总体难度随 round 上升；
	 - 鼠标点中得分，得分规则按色彩、大小、速度不同计算，规则可自由设定。

 - 游戏的要求：
 	- 使用带缓存的工厂模式管理不同飞碟的生产与回收，该工厂必须是场景单实例的！具体实现见参考资源 Singleton 模板类
 	- 尽可能使用前面 MVC 结构实现人机交互与游戏模型分离
## 设计思想
1.这次的打飞碟游戏能够重用之前牧师与恶魔的大部分代码，实现MVC的分离。例如SSAction的动作基类，ISSActionCallback的动作接口，导演类，门面模式，SSActionManager类等等。

2.有所改变的是，本游戏增加Disk的工厂类来减少新建和销毁的次数。还需要新增一个分数的管理者，来通过点击飞碟增加分数。

3.为什么需要工厂对象？游戏对象的创建与销毁高成本，必须减少销毁次数。如游戏中子弹。而且屏蔽创建与销毁的业务逻辑，使程序易于扩展。

根据UML图实现 ：
![在这里插入图片描述](https://img-blog.csdn.net/20180417220943517?watermark/2/text/aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2RpY2tkaWNrMTEx/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70)

## 代码实现部分

1.单例模式Singleton模板

```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    public static T Instance {  
        get {  
            if (instance == null) { 
                instance = (T)FindObjectOfType (typeof(T));  
                if (instance == null) {  
                    Debug.LogError ("An instance of " + typeof(T) +
                    " is needed in the scene, but there is none.");  
                }  
            }  
            return instance;  
        }  
    }
}
```
2.SSAction基类，飞碟的飞行继承这个类来实现

```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

//所有动作的基类
public class SSAction : ScriptableObject {
    public bool enable = true;
    public bool destroy = false;
    public GameObject gameobject { get; set; }
    public Transform transform { get; set; }
    public ISSActionCallback callback { get; set; }

    protected SSAction() { }
    // Use this for initialization
    public virtual void Start()
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        throw new System.NotImplementedException();
    }
    //重置函数
    public void reset()
    {
        enable = false;
        destroy = false;
        gameobject = null;
        transform = null;
        callback = null;
    }
}
```
3.CCMoveAction类，实际实现飞碟的飞行动作，平抛运动。

```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

//定义飞碟运动的动作，继承了SSAction的动作基类
public class CCMoveAction : SSAction {

    public float a;        //重力加速度
    public float v_x;      //水平运动速度
    public Vector3 dir;    //运动方向
    public float time;     //运动时间
    public static CCMoveAction GetSSAction()
    {
        CCMoveAction action = ScriptableObject.CreateInstance<CCMoveAction>();
        return action;
    }
    // Use this for initialization
    public override void Start () {
        enable = true;
        a = 9.8f;
        time = 0;
        v_x = gameobject.GetComponent<DiskComponent>().speed;
        dir = gameobject.GetComponent<DiskComponent>().direction;
    }

    // Update is called once per frame
    public override void Update () {
        if (gameobject.activeSelf)
        {
            time += Time.deltaTime;
            //竖直运动 v_y = a * time
            transform.Translate(Vector3.down * a * time * Time.deltaTime);
            //水平运动 v_x = v_x
            transform.Translate(dir * v_x * Time.deltaTime);
            //飞碟落地情况，将信息回调
            if (this.transform.position.y < -5)
            {
                this.destroy = true;
                this.enable = false;
                this.callback.SSActionEvent(this);
            }
        }
    }
}
```
4.SSActionManager类，CCActionManager类继承这个类来控制掌管飞碟的多个动作

```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

//创建 MonoBehaiviour 管理一个动作集合，动作做完自动回收动作。
public class SSActionManager : MonoBehaviour
{
    public Dictionary<int, SSAction> actions = new Dictionary<int, SSAction>();
    private List<SSAction> waitingAdd = new List<SSAction>();
    private List<int> waitingDelete = new List<int>();

    // 开始动作
    protected void Start()
    {
    }
    // 该类演示了复杂集合对象的使用。
    protected void Update()
    {
        foreach (SSAction ac in waitingAdd) actions[ac.GetInstanceID()] = ac;
        waitingAdd.Clear();

        foreach (KeyValuePair<int, SSAction> kv in actions)
        {
            SSAction ac = kv.Value;
            if (ac.destroy)
                waitingDelete.Add(ac.GetInstanceID());
            else if (ac.enable)
                ac.Update();
        }
        foreach (int key in waitingDelete)
        {
            SSAction ac = actions[key];
            actions.Remove(key);
            DestroyObject(ac);
        }
        waitingDelete.Clear();
    }
// 提供了运行一个新动作的方法。该方法把游戏对象与动作绑定，并绑定该动作事件的消息接收者。
    public void RunAction(GameObject gameobject, SSAction action, ISSActionCallback manager)
    {
        action.gameobject = gameobject;
        action.transform = gameobject.transform;
        action.callback = manager;
        action.destroy = false;
        action.enable = true;
        waitingAdd.Add(action);
        action.Start();
    }
}
```
5.CCActionManager类,利用两个链表，一个存放free，另一个存放used的飞碟。通过两个队列的入队出队实现控制。
```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class CCActionManager : SSActionManager,ISSActionCallback
{
    public FirstController sceneController;
    public List<CCMoveAction> Fly;
    public int DiskNumber = 0;              //统计飞碟个数

    private List<SSAction> used = new List<SSAction>();     // used是用来保存正在使用的动作
    private List<SSAction> free = new List<SSAction>();     // free是用来保存还未被激活的动作
    //如果free中没有动作就新建一个动作，否则直接从free中拿，这样可以减少destroy开销
    public SSAction GetSSAction()
    {
        SSAction action = null;
        if (free.Count > 0)
        {
            action = free[0];
            free.Remove(free[0]);
        }
        else
        {
            action = ScriptableObject.Instantiate<CCMoveAction>(Fly[0]);
        }
        used.Add(action);
        return action;
    }
    //free掉一个在used的动作
    public void FreeSSAction(SSAction action)
    {
        SSAction temp = null;
        foreach(SSAction i in used)
        {
            if (action.GetInstanceID() == i.GetInstanceID())
            {
                temp = i;
            }
        }
        if(temp != null)
        {
            temp.reset();
            free.Add(temp);
            used.Remove(temp);
        }
    }
    //实现接口ISSActionCallback
    public void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted, int intParam = 0, string strParam = null, Object objectParam = null)
    {
        if (source is CCMoveAction)
        {
            DiskNumber--;
            DiskFactory df = Singleton<DiskFactory>.Instance;
            df.FreeDisk(source.gameobject);
            FreeSSAction(source);
        }
    }
    protected new void Start()
    {
        sceneController = (FirstController)Director.getInstance().sceneController;
        sceneController.actionManager = this;
        Fly.Add(CCMoveAction.GetSSAction());
    }
    //扔飞盘的动作
    public void Throw(Queue<GameObject> diskQueue)
    {
        foreach (GameObject tmp in diskQueue)
        {
            RunAction(tmp, GetSSAction(), this);
        }
    }
}
```
6.BaseCode,与牧师恶魔一样放置导演类，门面模式的接口，动作与场景控制器的接口，游戏的状态,飞碟的基础属性等等。
```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

namespace Com.Mygame
{
    //导演类，单例模式
    public class Director : System.Object
    {
        private static Director D_instance;
        public SceneController sceneController { get; set; }

        public static Director getInstance()
        {
            if (D_instance == null)
                D_instance = new Director();
            return D_instance;
        }
    }

    //场景控制器，被FirstController类所继承来加载资源预设
    public interface SceneController
    {
        void load();
    }

    //负责界面与场景控制器通信的接口
    public enum GameState { ROUND_START, ROUND_FINISH, RUNNING, PAUSE, START ,GAME_OVER, WIN}

    public interface IUserAction
    {
        void GameOver();
        GameState getGameState();
        void setGameState(GameState gs);
        int GetScore();
        void hit(Vector3 pos);
    }

    //动作与动作管理器之间的接口
    public enum SSActionEventType : int { Started, Competeted }

    public interface ISSActionCallback
    {
        void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted,
            int intParam = 0, string strParam = null, Object objectParam = null);
    }

    //飞碟的一些基本属性，用于调整以及判断得分
    public class DiskComponent : MonoBehaviour
    {
        public Vector3 size;
        public int score;
        public float speed;
        public Vector3 direction;
    }

}
```
7.分数记录员ScoreRecorder类，set和get函数控制分数
```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

//得分记录者
public class ScoreRecorder : MonoBehaviour {
    private int score;

    private void Start()
    {
        score = 0;
    }

    public int getScore() { return score; }

    public void setScore(GameObject disk)
    {
        score += disk.GetComponent<DiskComponent>().score;
    }

    public void Reset()
    {
        score = 0;
    }
}
```
8.飞碟工厂类DiskFactory，与动作管理器相似利用两个队列来实现飞碟的出现与消失，并且根据round的不同生成不一样的飞碟种类。
```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

//用于生产飞碟的工厂
public class DiskFactory : MonoBehaviour {
    public GameObject diskPrefab;
    private List<DiskComponent> used = new List<DiskComponent>();     //used是用来保存正在使用的飞碟 
    private List<DiskComponent> free = new List<DiskComponent>();     //free是用来保存未激活的飞碟
    private int num = 1;
    //得到飞碟属性
    private int getCharacter(int round)
    {
        int character = 0;
        if (round == 0)
        {
            character = 0;
        }
        else if (round == 1)
        {
            character = Random.Range(-2f, 1f) > 0 ? 1 : 0;
        }
        else
        {
            character = Random.Range(-3f, 2f) > 0 ? 1 : 2;
        }
        return character;
    }
    //得到飞碟运动范围
    private float getStartX(int round)
    {
        if (round == 0)
        {
            return UnityEngine.Random.Range(-0.5f, 0.5f);
        }
        else if (round == 1)
        {
            return UnityEngine.Random.Range(-1.5f, 1.5f);
        }
        else
        {
            return UnityEngine.Random.Range(-2.5f, 2.5f);
        }
    }
    //先生成一个预制
    private void Awake()
    {
        diskPrefab = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Disk"), Vector3.zero, Quaternion.identity);
        diskPrefab.SetActive(false);
    }
    //生成一个飞碟
    public GameObject GetDisk(int round)
    {
        GameObject newdisk = null;
        if(free.Count > 0)
        {
            newdisk = free[0].gameObject;
            free.Remove(free[0]);
        }
        else
        {
            newdisk = GameObject.Instantiate<GameObject>(diskPrefab, Vector3.zero, Quaternion.identity);
            newdisk.AddComponent<DiskComponent>();
        }
        //通过关卡得到飞碟的属性
        int disk_character = getCharacter(round);
        //通过关卡得到飞碟的起始位置
        float RanX = getStartX(round);
        //选择生成飞碟的样式
        switch (disk_character)
        {
            case 0:
                {
                    newdisk.GetComponent<DiskComponent>().score = 1;
                    newdisk.GetComponent<DiskComponent>().speed = 5.0f;
                    newdisk.GetComponent<DiskComponent>().direction = new Vector3(RanX, 1.5f, 0);
                    newdisk.GetComponent<Renderer>().material.color = Color.white;
                    break;
                }
            case 1:
                {
                    newdisk.GetComponent<DiskComponent>().score = 2;
                    newdisk.GetComponent<DiskComponent>().speed = 6.0f;
                    newdisk.GetComponent<DiskComponent>().direction = new Vector3(RanX, 1, 0);
                    newdisk.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                }
            case 2:
                {
                    newdisk.GetComponent<DiskComponent>().score = 3;
                    newdisk.GetComponent<DiskComponent>().speed = 7.0f;
                    newdisk.GetComponent<DiskComponent>().direction = new Vector3(RanX, 0.5f, 0);
                    newdisk.GetComponent<Renderer>().material.color = Color.red;
                    break;
                }
        }
        used.Add(newdisk.GetComponent<DiskComponent>());
        newdisk.name = "disk_" + num;
        num++;
        return newdisk;
    }
    //free掉已用过的飞碟
    public void FreeDisk(GameObject disk)
    {
        DiskComponent temp = null;
        foreach (DiskComponent i in used)
        {
            if (disk.GetInstanceID() == i.gameObject.GetInstanceID())
            {
                temp = i;
            }
        }
        if(temp != null)
        {
            temp.gameObject.SetActive(false);
            free.Add(temp);
            used.Remove(temp);
        }
    }

}
```
9.UserGUI类，利用OnGUI实现用户界面展示游戏开始，结束，胜利等状态
```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class UserGUI : MonoBehaviour
{
    private IUserAction action;
    bool isFirst = true;
    // Use this for initialization  
    void Awake()
    {
        action = Director.getInstance().sceneController as IUserAction;
        isFirst = true;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(500, 10, 300, 300), "Rule:\nHit White Disk,  You will get 1 point\nHit Yellow Disk, You will get 2 point" +
            "\nHit Red Disk,     You will get 3 point" +
            "\nFirst target: 5\nSecond target: 15\nFinal target: 30");
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 pos = Input.mousePosition;
            action.hit(pos);
        }
        GUI.Label(new Rect(20, 10, 400, 400), "Score : "+ action.GetScore().ToString());
        if (isFirst && GUI.Button(new Rect(350, 140, 100, 100), "Start"))
        {
            isFirst = false;
            action.setGameState(GameState.ROUND_START);
        }
        if (!isFirst && action.getGameState() == GameState.ROUND_FINISH && GUI.Button(new Rect(350, 140, 100, 100), "Next Round"))
        {
            action.setGameState(GameState.ROUND_START);
        }
        if(action.getGameState() == GameState.GAME_OVER)
        {
            GUI.Label(new Rect(366, 330, 90, 90), "GAMEOVER!");
            if (GUI.Button(new Rect(350, 140, 100, 100), "ReStart"))
            {
                isFirst = false;
                action.setGameState(GameState.ROUND_START);
            }
        }
        if (action.getGameState() == GameState.WIN)
        {
            GUI.Label(new Rect(366, 330, 90, 90), "YOU WIN!");
            if (GUI.Button(new Rect(350, 140, 100, 100), "ReStart"))
            {
                isFirst = false;
                action.setGameState(GameState.ROUND_START);
            }
        }
    }
}
```
10.最为重要的一个类，FirstController类是场景控制器，负责控制游戏的状态，控制飞碟的飞行，鼠标的点击事件。
```cpp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class FirstController : MonoBehaviour, SceneController, IUserAction
{
    public CCActionManager actionManager;
    public ScoreRecorder scoreRecorder;
    public Queue<GameObject> diskQueue = new Queue<GameObject>();
    private int diskNumber;
    private int currentRound = -1;
    public int round = 3;
    private float time = 0;
    private GameState gameState = GameState.START;

    //鼠标点击事件
    public void hit(Vector3 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider.gameObject.GetComponent<DiskComponent>() != null)
            {
                scoreRecorder.setScore(hit.collider.gameObject);
                /*如果飞碟被击中，那么就移到地面之下，由工厂负责回收 */
                hit.collider.gameObject.transform.position = new Vector3(0, -6, 0);
            }
        }
    }
    //飞碟飞行动作
    void ThrowDisk()
    {
        if (diskQueue.Count != 0)
        {
            GameObject disk = diskQueue.Dequeue();
            /*确定飞碟出现的位置 */
            Vector3 position = new Vector3(0, 0, 0);
            float y = UnityEngine.Random.Range(0f, 4f);
            position = new Vector3(-disk.GetComponent<DiskComponent>().direction.x * 7, y, 0);
            disk.transform.position = position;
            disk.SetActive(true);
        }

    }

    void Awake()
    {
        Director director = Director.getInstance();
        director.sceneController = this;
        diskNumber = 10;
        this.gameObject.AddComponent<ScoreRecorder>();
        this.gameObject.AddComponent<DiskFactory>();
        scoreRecorder = Singleton<ScoreRecorder>.Instance;
        director.sceneController.load();
    }
    int[] point = new int[4] { 1, 5, 15, 30 };
    int num = 0;
    private void Update()
    {
        //游戏结束
        if (scoreRecorder.getScore() <= point[num] && gameState == GameState.ROUND_FINISH)
        {
            //再次初始化
            currentRound = -1;
            round = 3;
            time = 0;
            num = 0;
            gameState = GameState.GAME_OVER;
            scoreRecorder.Reset();
        }
        if (scoreRecorder.getScore() >= 30 && gameState == GameState.ROUND_FINISH)
        {       
            //再次初始化
            currentRound = -1;
            round = 3;
            time = 0;
            num = 0;
            scoreRecorder.Reset();
            gameState = GameState.WIN;
        }
            //回合结束
        if (actionManager.DiskNumber == 0 && gameState == GameState.RUNNING)
        {
            gameState = GameState.ROUND_FINISH;
        }
        //回合开始
        if (actionManager.DiskNumber == 0 && gameState == GameState.ROUND_START)
        {
            currentRound = (currentRound + 1) % round;
            num++;
            DiskFactory df = Singleton<DiskFactory>.Instance;
            for (int i = 0; i < diskNumber; i++)
            {
                diskQueue.Enqueue(df.GetDisk(currentRound));
            }
            actionManager.Throw(diskQueue);
            actionManager.DiskNumber = 10;
            gameState = GameState.RUNNING;
        }
        if (time > 0.5)
        {
            ThrowDisk();
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }
        Debug.Log(num);
    }
    //游戏资源的加载
    public void load()
    {

    }
    public void GameOver()
    {
        setGameState(GameState.GAME_OVER);
    }
    public int GetScore()
    {
        return scoreRecorder.getScore();
    }

    public GameState getGameState()
    {
        return gameState;
    }

    public void setGameState(GameState gs)
    {
        gameState = gs;
    }
}
```
游戏效果图：

![在这里插入图片描述](https://img-blog.csdn.net/20180417221013531?watermark/2/text/aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2RpY2tkaWNrMTEx/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70)
