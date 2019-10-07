using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;


namespace Com.Mygame
{
    public class CCActionManager: MonoBehaviour, ActionCallBack {
        private Dictionary<int, SSAction> actions = new Dictionary<int, SSAction>();
        private List<SSAction> waitingToAdd = new List<SSAction>();
        private List<int> watingToDelete = new List<int>();

        protected void Update() {
            foreach(SSAction ac in waitingToAdd) {
                actions[ac.GetInstanceID()] = ac;
            }
            waitingToAdd.Clear();

            foreach(KeyValuePair<int, SSAction> kv in actions) {
                SSAction ac = kv.Value;
                if (ac.destroy) {
                    watingToDelete.Add(ac.GetInstanceID());
                } else if (ac.enable) {
                    ac.Update();
                }
            }

            foreach(int key in watingToDelete) {
                SSAction ac = actions[key];
                actions.Remove(key);
                DestroyObject(ac);
            }
            watingToDelete.Clear();
        }

        public void addAction(GameObject gameObject, SSAction action, ActionCallBack whoToNotify) {
            action.gameObject = gameObject;
            action.transform = gameObject.transform;
            action.whoToNotify = whoToNotify;
            waitingToAdd.Add(action);
            action.Start();
        }

        public void SSActionEvent(SSAction source) {
            
        }

    }

    
}