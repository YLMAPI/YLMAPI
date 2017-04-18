using System;
using System.Text;
using UnityEngine;

namespace SGUI {
    public class SGroupMinimumContentSizeModifier : SModifier {

        public override void UpdateStyle() {
            SGroup group = (SGroup) Elem;
            group.ContentSize = Vector2.zero;
            for (int i = group.Children.Count - 1; i > -1; --i)
                group.GrowToFit(group[i]);
        }

    }
}
