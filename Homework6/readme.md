**演示视频地址：[http://www.iqiyi.com/w_19rxsmp5kx.html](http://www.iqiyi.com/w_19rxsmp5kx.html)**

一. 游戏要求

智能巡逻兵游戏内容要求：

 1. 每个巡逻兵走一个3~5个边的凸多边型，位置数据是相对地址。即每次确定下一个目标位置，用自己当前位置为原点计算；
 2. 巡逻兵碰撞到障碍物，则会自动选下一个点为目标；
 3. 巡逻兵在设定范围内感知到玩家，会自动追击玩家；
 4. 失去玩家目标后，继续巡逻；
 5. 计分：玩家每次甩掉一个巡逻兵计一分，与巡逻兵碰撞游戏结束；
 6. 创建一个地图和若干巡逻兵(使用动画)；

智能巡逻兵游戏程序设计要求：

 

 1. 必须使用订阅与发布模式传消
 2.  subject：OnLostGoal
 3. Publisher: ?
 4. Subscriber: ?
 5. 工厂模式生产巡逻兵

二. 设计思路
对于这个巡逻兵游戏，原本的MVC模式肯定是必备的，由于之前的读书笔记上有说明，在此就不详述。而新添加的订阅与发布模式则是为编程提供了不少方便，引用一下潘老师的原话，“随着程序代码不断变多，“解耦”或使得每个部分相对独立的技术就更重要了。”由此可见订阅与发布模式的确十分重要。
```
A. 订阅与发布模式
Q: 什么是订阅与发布模式？
A: 适在观察者模式中的Subject就像一个发布者（Publisher），而观察者（Observer）完全可以看作一个订阅者（Subscriber）。subject通知观察者时，就像一个发布者通知他的订阅者。
```
![在这里插入图片描述](https://img-blog.csdnimg.cn/20191026125653951.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2FkZ2hqZ2Y=,size_16,color_FFFFFF,t_70)

在了解学习了订阅与发布模式后，我仔细考虑了一下在这个游戏中哪些地方可以使用呢。答案就是当一个巡逻兵捕捉到玩家后，GameEventManager就应该发布一个游戏结束的信息，但它并不知道谁会订阅这个消息。然后firstController中订阅该消息，知道游戏结束后调用相应的组件来实现游戏结束的功能。还有玩家摆脱巡逻兵的追捕，分数需要叠加的时候同样适用。这样的使用免去了firstController直接对每个巡逻兵玩家的判断。

B. 动画与碰撞器的设置，合理使用资源
这个巡逻兵游戏需要我们要掌握驾驭资源的能力，对于一个预制来进行操作，其中不仅包括简单的位置，大小，还有较为复杂的动画切换，碰撞器设置。

对于动画切换的部分，要善于利用Animator功能，通过变量的改变来确定某一个时刻具体执行某一动作。如下图，当run变量为true时，玩家就会执行跑步动作，切换至跑步状态。
![在这里插入图片描述](https://img-blog.csdn.net/20180511003405871)

而对于碰撞器的设置也是这个游戏的关键之处，如何让每个角色都有自己特定的功能呢，这就需要给每个预制添加上合适的脚本函数与碰撞器。这一部分，我利用了OnCollisionEnter函数来判断巡逻兵与玩家碰撞，当碰撞发生时，证明玩家被捕捉。OnTriggerEnter()函数则是用来判断巡逻兵附近是否有玩家存在，是否应该追击。

这里为什么不用OnCollisionEnter，因为巡逻兵距离玩家应该存在一定的距离，而不是应该直接被碰撞，故使用trigger。而且我们在巡逻兵添加这个脚本的碰撞器的大小应该大于巡逻兵的身材，只有这样它才能感知附近的玩家。
![在这里插入图片描述](https://img-blog.csdn.net/20180511004201764)

三. 具体代码实现
1.GameEventManager类用于实现发布与订阅模式，里面发布分数增加信息以及游戏结束的信息。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//消息的中间场所，firstController订阅该类得到信息执行对应的操作
public class GameEventManager : MonoBehaviour
{
    //接收分数增加消息
    public delegate void ScoreEvent();
    public static event ScoreEvent ScoreChange;
    //接收游戏结束消息
    public delegate void GameoverEvent();
    public static event GameoverEvent GameoverChange;

    public void AddScore()
    {
        if (ScoreChange != null)
        {
            ScoreChange();
        }
    }

    public void Gameover()
    {
        if (GameoverChange != null)
        {
            GameoverChange();
        }
    }
}
```
2.AreaCollide，，这三个脚本分别加载至玩家，巡逻兵，地形中，用于判断碰撞器的碰撞情况，做出相对应的行动。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

//区域碰撞器脚本。当玩家进入自己的区域,碰撞一发生马上改变sign
public class AreaCollide : MonoBehaviour
{
    public int sign = 0;
    FirstController sceneController = (FirstController) Director.getInstance().sceneController;
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            sceneController.area_sign = sign;
        }
    }
}

//巡逻兵碰撞器脚本，用于判断是否追踪玩家
public class PatrolCollide : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.gameObject.tag);
        if (collider.gameObject.tag == "Player")
        {
            this.gameObject.transform.parent.GetComponent<PatrolData>().player = collider.gameObject;
            this.gameObject.transform.parent.GetComponent<PatrolData>().follow_player = true;
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            this.gameObject.transform.parent.GetComponent<PatrolData>().player = null;
            this.gameObject.transform.parent.GetComponent<PatrolData>().follow_player = false;
        }
    }
}

//玩家碰撞器脚本，用于判断游戏是否结束
public class PlayerCollide : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Animator>().SetBool("run", false);
            this.GetComponent<Animator>().SetBool("run", false);
            other.gameObject.GetComponent<Animator>().SetTrigger("death");
            this.GetComponent<Animator>().SetTrigger("attack");
            //发布游戏结束消息
            Singleton<GameEventManager>.Instance.Gameover();
        }
    }
}
```
3.Singleton类，用于实现单例模式，与之前的游戏代码一致
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));
                if (instance == null)
                {
                    Debug.LogError("An instance of " + typeof(T)
                        + " is needed in the scene, but there is none.");
                }
            }
            return instance;
        }
    }
}
```
4.SSAction类，角色的动作基类。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class SSAction : ScriptableObject
{
    public bool enable = true;                      
    public bool destroy = false;                    
    public GameObject gameobject;                   
    public Transform transform;                     
    public ISSActionCallback callback;              //动作完成后的消息通知者

    protected SSAction() { }
    public virtual void Start()
    {
        throw new System.NotImplementedException();
    }
    public virtual void Update()
    {
        throw new System.NotImplementedException();
    }
}

5.SSActionManger类，角色的动作管理器基类，用于销毁动作以及决定执行哪些动作。

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class SSActionManager : MonoBehaviour
{
    public Dictionary<int, SSAction> actions = new Dictionary<int, SSAction>();    //将执行的动作的字典集合
    private List<SSAction> waitingAdd = new List<SSAction>();                       //等待去执行的动作列表
    private List<int> waitingDelete = new List<int>();                              //等待删除的动作的key                

    // 开始动作
    protected void Start()
    {
    }
    protected void Update()
    {
        foreach (SSAction ac in waitingAdd)
        {
            actions[ac.GetInstanceID()] = ac;
        }
        waitingAdd.Clear();

        foreach (KeyValuePair<int, SSAction> kv in actions)
        {
            SSAction ac = kv.Value;
            if (ac.destroy)
            {
                waitingDelete.Add(ac.GetInstanceID());
            }
            else if (ac.enable)
            {
                ac.Update();
            }
        }
        foreach (int key in waitingDelete)
        {
            SSAction ac = actions[key];
            actions.Remove(key);
            DestroyObject(ac);
        }
        waitingDelete.Clear();
    }

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
6.UserGUI用于显示以及更改分数，在这个游戏中还需要读取玩家的键盘输入，来控制玩家的移动。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class UserGUI : MonoBehaviour {
    private IUserAction action;
    void Start ()
    {
        action = Director.getInstance().sceneController as IUserAction;
    }

    void Update()
    {
        float translationX = Input.GetAxis("Horizontal");
        float translationZ = Input.GetAxis("Vertical");
        //移动玩家
        action.playerMove(translationX, translationZ);
    }
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 5, 200, 50), "分数:");
        GUI.Label(new Rect(55, 5, 200, 50), action.getScore().ToString());
        if(action.getGameover())
        {
            GUI.Label(new Rect(Screen.width / 2-40, Screen.width / 2 - 200, 100, 100), "Game over!\nYour score is " + action.getScore().ToString());
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.width / 2 - 150, 100, 40), "restart"))
            {
                action.restart();
                return;
            }
        }
    }
}
```
7.BaseCode里面定义了导演的单例模式，巡逻兵的信息类，以及一些接口，其中包括动作回调接口，界面交互的接口。
```
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
    //用户界面接口
    public interface IUserAction
    {
        //通过键盘输入移动玩家
        void playerMove(float translationX, float translationZ);
        //得到分数
        int getScore();
        //得到游戏结束标志
        bool getGameover();
        //重新开始
        void restart();
    }
    //动作回调接口
    public interface ISSActionCallback
    {
        void SSActionEvent(SSAction source, int intParam = 0, GameObject objectParam = null);
    }
    //巡逻兵的属性
    public class PatrolData : MonoBehaviour
    {
        public int area_sign = -1;            //当前玩家Player所在区域标志
        public GameObject player;             //Player游戏对象
        public int sign;                      //标志巡逻兵在哪一块区域
        public bool follow_player = false;    //跟随玩家标志
        public Vector3 start_position;        //巡逻兵的初始位置     
    }

}
```
8.ScoreRecorder用于记录分数以及返回分数的信息。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class ScoreRecorder : MonoBehaviour
{
    public FirstController sceneController;
    public int score = 0;      

    void Start()
    {
        sceneController = (FirstController)Director.getInstance().sceneController;
        sceneController.recorder = this;
    }
    public int getScore()
    {
        return score;
    }
    public void addScore()
    {
        score++;
    }
}
```

9.巡逻兵的工厂，在该游戏由于巡逻兵不会死亡，故不需要回收队列来回收巡逻兵。在该工厂中，我们需要定义巡逻兵的初始的位置，以及定义一个停止巡逻兵的函数。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class PatrolFactory : MonoBehaviour
{
    private GameObject patrol = null;                              //巡逻兵
    private List<GameObject> used = new List<GameObject>();        //正在被使用的巡逻兵
    private Vector3[] vec = new Vector3[9];                        //保存每个巡逻兵的初始位置

    public List<GameObject> GetPatrols()
    {
        int[] pos_x = { -11, -1, 8 };
        int[] pos_z = { -3, 6, -13 };
        int index = 0;
        //生成不同的巡逻兵初始位置
        for(int i=0;i < 3;i++)
        {
            for(int j=0;j < 3;j++)
            {
                vec[index] = new Vector3(pos_x[i], 0, pos_z[j]);
                index++;
            }
        }
        for(int i=0; i < 9; i++)
        {
            patrol = Instantiate(Resources.Load<GameObject>("Prefabs/Patrol"));
            patrol.transform.position = vec[i];
            patrol.AddComponent<PatrolData>();
            patrol.GetComponent<PatrolData>().sign = i + 1;
            patrol.GetComponent<PatrolData>().start_position = vec[i];
            used.Add(patrol);
        }

        return used;
    }

    //停止巡逻兵的动画
    public void StopPatrol()
    {
        for (int i = 0; i < used.Count; i++)
        {
            used[i].gameObject.GetComponent<Animator>().SetBool("run", false);
        }
    }
}
```
10.PatrolFollowAction类巡逻兵的跟踪玩家时的动作，此时是需要一个较快的速度以及玩家的位置。当玩家逃脱时，必须销毁该动作。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class PatrolFollowAction : SSAction
{
    private GameObject player;           
    private PatrolData data;

    public override void Start()
    {
        data = this.gameobject.GetComponent<PatrolData>();
    }
    public override void Update()
    {
        transform.position = Vector3.MoveTowards(this.transform.position, player.transform.position, 2.5f * Time.deltaTime);
        this.transform.LookAt(player.transform.position);
        //玩家逃脱或者不跟随的巡逻情况，销毁该动作
        if (!data.follow_player || data.area_sign != data.sign)
        {
            this.destroy = true;
            this.callback.SSActionEvent(this,1,this.gameobject);
        }
    }
    public static PatrolFollowAction GetSSAction(GameObject player)
    {
        PatrolFollowAction action = CreateInstance<PatrolFollowAction>();
        action.player = player;
        return action;
    }

}
```
11.巡逻兵普通的巡逻动作，由于题目要求必须矩形运动，所以这里的运动必须经过四个方向的判断。当玩家出现在巡逻兵附近时，必须销毁该动作。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class PatrolMoveAction : SSAction
{
    private enum Dirction {  WEST, NORTH, EAST, SOUTH};
    private Dirction dirction = Dirction.WEST;  //移动的方向
    private PatrolData data;                    //巡逻兵的数据
    private float pos_x, pos_z;                 //巡逻兵移动前的初始坐标
    private float move_length;                  //巡逻兵移动的长度
    private bool is_turn = true;

    public override void Start()
    {
        data = this.gameobject.GetComponent<PatrolData>();
    }
    public override void Update()
    {
        patrolMove();
        //巡逻兵跟随，销毁该动作
        if (data.follow_player && data.area_sign == data.sign)
        {
            this.destroy = true;
            this.callback.SSActionEvent(this,0,this.gameobject);
        }
    }
    //返回一个动作，继承SSAction
    public static PatrolMoveAction GetSSAction(Vector3 location)
    {
        PatrolMoveAction action = CreateInstance<PatrolMoveAction>();
        action.pos_x = location.x;
        action.pos_z = location.z;
        //设定移动矩形的边长
        action.move_length = Random.Range(3, 5);
        return action;
    }
    //按照随机矩形轨迹运动
    void patrolMove()
    {
        if (is_turn)
        {
            switch (dirction)
            {
                case Dirction.EAST:
                    pos_x -= move_length;
                    break;
                case Dirction.NORTH:
                    pos_z += move_length;
                    break;
                case Dirction.WEST:
                    pos_x += move_length;
                    break;
                case Dirction.SOUTH:
                    pos_z -= move_length;
                    break;
            }
            is_turn = false;
        }
        this.transform.LookAt(new Vector3(pos_x, 0, pos_z));
        //比较两点的距离
        float distance = Vector3.Distance(transform.position, new Vector3(pos_x, 0, pos_z));
        //巡逻兵移动
        if (distance > 0.9)
        {
            transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(pos_x, 0, pos_z), 1.5f * Time.deltaTime);
        }
        //巡逻兵改变方向
        else
        {
            dirction = dirction + 1;
            if(dirction > Dirction.SOUTH)
            {
                dirction = Dirction.WEST;
            }
            is_turn = true;
        }
    }
}
```
12.PatrolActionManager类继承动作管理器基类，实现回调接口。通过参数的不同来决定执行哪一个动作，是普通巡逻还是追踪，而且需要销毁另一个动作。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class PatrolActionManager : SSActionManager, ISSActionCallback
{
    //巡逻兵巡逻
    private PatrolMoveAction patrolmove;                            
    public void patrolMove(GameObject patrol)
    {
        patrolmove = PatrolMoveAction.GetSSAction(patrol.transform.position);
        this.RunAction(patrol, patrolmove, this);
    }
    //停止所有动作
    public void DestroyAllAction()
    {
        foreach (KeyValuePair<int, SSAction> kv in actions)
        {
            SSAction ac = kv.Value;
            ac.destroy = true;
        }
    }
    //根据传入参数来选择执行不同的动作
    public void SSActionEvent(SSAction source, int intParam = 0, GameObject objectParam = null)
    {
        if (intParam == 0)
        {
            //侦查兵跟随玩家
            PatrolFollowAction follow = PatrolFollowAction.GetSSAction(objectParam.gameObject.GetComponent<PatrolData>().player);
            this.RunAction(objectParam, follow, this);
        }
        else
        {
            //侦察兵按照初始位置开始继续巡逻
            PatrolMoveAction move = PatrolMoveAction.GetSSAction(objectParam.gameObject.GetComponent<PatrolData>().start_position);
            this.RunAction(objectParam, move, this);
            //玩家逃脱
            Singleton<GameEventManager>.Instance.AddScore();
        }
    }
}
```
13.FirstController类是控制整个场景的首脑，负责订阅消息，当接收的消息后通知其他组件做相应的动作。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Com.Mygame;

public class FirstController : MonoBehaviour, IUserAction, SceneController
{
    public PatrolFactory patrol_factory;                 
    public ScoreRecorder recorder;                               
    public PatrolActionManager action_manager;                      
    public int area_sign;
    public GameObject player;                 
    private List<GameObject> patrols;                        
    private bool game_over = false;                
    //实例化组件，加载资源
    void Start()
    {
        Director.getInstance().sceneController = this;
        patrol_factory = Singleton<PatrolFactory>.Instance;
        action_manager = (PatrolActionManager)gameObject.AddComponent<PatrolActionManager>();
        recorder = Singleton<ScoreRecorder>.Instance;
        load();
    }

    //每帧更新玩家位置，通知巡逻兵
    void Update()
    {
        for (int i = 0; i < patrols.Count; i++)
        {
            patrols[i].gameObject.GetComponent<PatrolData>().area_sign = area_sign;
        }
    }
    //订阅模式，分别订阅分数增加与游戏结束消息
    void OnEnable()
    {
        GameEventManager.ScoreChange += AddScore;
        GameEventManager.GameoverChange += Gameover;
    }
    void OnDisable()
    {
        GameEventManager.ScoreChange -= AddScore;
        GameEventManager.GameoverChange -= Gameover;
    }
    void AddScore()
    {
        recorder.addScore();
    }
    void Gameover()
    {
        game_over = true;
        patrol_factory.StopPatrol();
        action_manager.DestroyAllAction();
    }
    public void load()
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/Plane"));
        //player从高处掉落，才能产生碰撞
        player = Instantiate(Resources.Load("Prefabs/Player"), new Vector3(0, 5, 0), Quaternion.identity) as GameObject;
        patrols = patrol_factory.GetPatrols();
        //所有侦察兵移动
        for (int i = 0; i < patrols.Count; i++)
        {
            action_manager.patrolMove(patrols[i]);
        }
    }
    //实现接口的IUserAction的四个函数
    //玩家移动
    public void playerMove(float translationX, float translationZ)
    {
        //游戏未结束
        if(!game_over)
        {
            //设置动画
            if (translationX != 0 || translationZ != 0)
            {
                player.GetComponent<Animator>().SetBool("run", true);
            }
            //移动和旋转
            player.transform.Translate(translationX * 4f * Time.deltaTime, 0, translationZ * 4f * Time.deltaTime);
            player.transform.Rotate(0, translationX * 100f * Time.deltaTime, 0);
            //防止碰撞带来的移动
            if (player.transform.localEulerAngles.x != 0 || player.transform.localEulerAngles.z != 0)
            {
                player.transform.localEulerAngles = new Vector3(0, player.transform.localEulerAngles.y, 0);
            }
            if (player.transform.position.y != 0)
            {
                player.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
            }
        }
    }
    public int getScore()
    {
        return recorder.getScore();
    }
    public bool getGameover()
    {
        return game_over;
    }
    //重置函数
    public void restart()
    {
        SceneManager.LoadScene("Scenes/mySence");
    }
}
```
四. 总结
完成这个游戏的修改，我收获了游戏资源的使用，碰撞器的调整，动画的切换，消息的传递，还学习了新的设计模式——订阅与发布模式，对于游戏设计也有了更深一层的体验。

游戏效果图如下:

![在这里插入图片描述](https://img-blog.csdn.net/20180511010914728)
