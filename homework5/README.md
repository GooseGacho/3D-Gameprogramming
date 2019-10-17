本文利用上次所完成的打飞碟游戏的基础上，利用物理引擎以及Adapter模式来改写打飞碟
游戏演示视频地址：http://www.iqiyi.com/w_19rxytfwlx.html
<html><hr><html>
一. 游戏要求
改进飞碟（Hit UFO）游戏：

游戏内容要求：
1. 按 adapter模式 设计图修改飞碟游戏
2. 使它同时支持物理运动与运动学（变换）运动

二. 设计思路
打飞碟游戏-基础版
由于这次的改动是建立在之前的游戏基础上，我们先要搞清楚哪些类是可以重用的，哪些类需要我们新增加的。关于物理引擎的动作改动肯定离不开对于动作类的更改，在此我们引入Adapter模式来帮助我们更好的更改这些类。

Q: 什么是Adapter模式？
A: 适配器模式（Adapter Pattern）是作为两个不兼容的接口之间的桥梁。这种类型的设计模式属于结构型模式，它结合了两个独立接口的功能。
这种模式涉及到一个单一的类，该类负责加入独立的或不兼容的接口功能。举个真实的例子，读卡器是作为内存卡和笔记本之间的适配器。您将内存卡插入读卡器，再将读卡器插入笔记本，这样就可以通过笔记本来读取内存卡。

所以，我们根据适配器模式的思路分析这个游戏动作类，发现只需要把之前动力学的CCActionManger, CCMoveAction 增加一个适配器接口，也就是IActionManger。再增加类似的两个类PhysicActionManger, PhysicMoveAction即可。总而言之，这次的改动只需要增加这三个类，算是比较简单的。
![在这里插入图片描述](https://img-blog.csdn.net/20180424185120965?watermark/2/text/aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2RpY2tkaWNrMTEx/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70)

三. 具体代码实现
1.适配器接口类IActionManager。根据Adapter模式，IActionManager接口被物理学动作管理器与动力学动作管理器，而实现这些接口可以查看已经实现的CCActionManager中需要哪些实现。
```c
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Adapter模式，IActionManager接口被物理学动作管理器与动力学动作管理器实现
//这些接口可以查看已经实现的CCActionManager中需要哪些实现
public interface IActionManager {
    void Throw(Queue<GameObject> diskQueue);
    //更改后，无法再直接访问DiskNumber,故需要两个接口函数来访问
    int getDiskNumber();
    void setDiskNumber(int num);
}
```
2.PhysicMoveAction类，与动力学的动作类差不多，也是要基础SSAction基类。唯一的区别是利用的是物体的刚体属性，以及物理引擎实现运动。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class PhysicMoveAction : SSAction {
    Vector3 force;//力  
    public SceneController sceneControler = (SceneController)Director.getInstance().sceneController;
    public static PhysicMoveAction GetSSAction()
    {
        PhysicMoveAction action = ScriptableObject.CreateInstance<PhysicMoveAction>();
        return action;
    }
    public override void Start()
    {
        //初始化及射出的力度
        force = new Vector3(2 * Random.Range(-1, 1), -2.5f * Random.Range(0.5f, 2), -1 + 2 * Random.Range(0.5f, 2));//力的大小  
    }
    public override void Update()
    {
        if (gameobject.activeSelf)
        {
            //将CCMoveAction中的transform改为刚体，加上力
            Debug.Log(this.transform.position.y);
            //如果物件没有刚体属性，则为它加上刚体属性
            if(gameobject.GetComponent<Rigidbody>() == null)
                gameobject.AddComponent<Rigidbody>();
            gameobject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameobject.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
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
3.PhysicActionManager类，与CCActionManager类差不多，这两个类都要实现适配器接口函数。在FirstController类中决定使用哪一个类中的接口函数。
```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

public class PhysicActionManager : SSActionManager, ISSActionCallback, IActionManager
{
    public FirstController sceneController;
    public List<PhysicMoveAction> Fly;
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
            action = ScriptableObject.Instantiate<PhysicMoveAction>(Fly[0]);
        }
        used.Add(action);
        return action;
    }
    //free掉一个在used的动作
    public void FreeSSAction(SSAction action)
    {
        SSAction temp = null;
        foreach (SSAction i in used)
        {
            if (action.GetInstanceID() == i.GetInstanceID())
            {
                temp = i;
            }
        }
        if (temp != null)
        {
            temp.reset();
            free.Add(temp);
            used.Remove(temp);
        }
    }
    //实现接口ISSActionCallback
    public void SSActionEvent(SSAction source, SSActionEventType events = SSActionEventType.Competeted, int intParam = 0, string strParam = null, Object objectParam = null)
    {
        if (source is PhysicMoveAction)
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
        if (sceneController.is_physics == true)
            sceneController.actionManager = this;
        Fly.Add(PhysicMoveAction.GetSSAction());
    }
    //扔飞盘的动作
    public void Throw(Queue<GameObject> diskQueue)
    {
        foreach (GameObject tmp in diskQueue)
        {
            RunAction(tmp, GetSSAction(), this);
        }
    }
    public int getDiskNumber()
    {
        return DiskNumber;
    }
    public void setDiskNumber(int num)
    {
        DiskNumber = num;
    }
}
```
4.FirstController类的修改，添加is_physics布尔变量用于决定使用哪个动作管理器类，可以在游戏开始前决定。
```
public class FirstController : MonoBehaviour, SceneController, IUserAction
{
    public IActionManager actionManager { get; set; }  //根据情况选择动作管理器
    public bool is_physics; //决定使用哪个动作管理器
    ······
    void Awake()
    {
        Director director = Director.getInstance();
        //is_physics = false;
        director.sceneController = this;
        diskNumber = 10;
        currentRound = -1;
        this.gameObject.AddComponent<ScoreRecorder>();
        this.gameObject.AddComponent<DiskFactory>();
        scoreRecorder = Singleton<ScoreRecorder>.Instance;
        director.sceneController.load();
    }
```
四. 总结
完成这个游戏的修改，我收获了游戏对象的刚体对象的使用方式，还学习了新的设计模式——适配器模式，对于游戏设计也有了更深一层的体验。

游戏效果图如下:
![在这里插入图片描述](https://img-blog.csdn.net/20180424190811997?watermark/2/text/aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L2RpY2tkaWNrMTEx/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70)
