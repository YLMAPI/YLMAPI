#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using System.IO;
using UnityEngine;
using YLMAPI;

class patch_SystemManager : SystemManager {

    /*
    private extern void orig_Awake();
    private void Awake() {
        orig_Awake();
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
        if (!(this is GenericSystemManager)) {
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
            Destroy(this);
            gameObject.AddComponent<GenericSystemManager>();
            return;
        }
    }
    */

}
