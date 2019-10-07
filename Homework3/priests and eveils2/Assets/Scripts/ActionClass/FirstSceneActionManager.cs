using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.Mygame;

namespace Com.Mygame {
    public class FirstSceneActionManager:CCActionManager {
        public void moveBoat(BoatController boat) {
		    CCMoveToAction action = CCMoveToAction.getSSAction(boat.getDestination(), boat.movingSpeed);
		    this.addAction(boat.getGameobj(), action, this);
        }

		

        public void moveCharacter(MyCharacterController characterCtrl, Vector3 destination) {
			Vector3 currentPos = characterCtrl.getPos();
			Vector3 middlePos = currentPos;
			if (destination.y > currentPos.y) {		//from low(boat) to high(coast)
				middlePos.y = destination.y;
			} else {	//from high(coast) to low(boat)
				middlePos.x = destination.x;
			}
			SSAction action1 = CCMoveToAction.getSSAction(middlePos, characterCtrl.movingSpeed);
			SSAction action2 = CCMoveToAction.getSSAction(destination, characterCtrl.movingSpeed);
			SSAction seqAction = CCSequenceAction.getSSAction(1, 0, new List<SSAction>{action1, action2});
			this.addAction(characterCtrl.getGameobj(), seqAction, this);
        }
    }
}