using System;
using System.Text;
using UnityEngine;

namespace SGUI {
    public class SGroupForceScrollModifier : SModifier {

        public override void Update() {
            SGroup group = (SGroup) Elem;
            if (group.ContentSize.y < group.Size.y)
                group.ContentSize = new Vector2(group.ContentSize.x, group.Size.y + group.GrowExtra.y);
        }

    }
}
