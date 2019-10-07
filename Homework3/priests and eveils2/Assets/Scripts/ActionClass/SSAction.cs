using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;


namespace Com.Mygame
{
    public class SSAction : ScriptableObject
    {

		public bool enable = true;
		public bool destroy = false;

		public GameObject gameObject;
        public Transform transform;
        public ActionCallBack whoToNotify;

        public virtual void Start()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Update()
        {
            throw new System.NotImplementedException();
        }
    }

    
}