using System;
using System.Text;
using UnityEngine;

namespace SGUI {
    public class SCheckboxModifier : SModifier {

        public static Texture2D DefaultChecked;
        public static Texture2D DefaultUnchecked;
        public static Vector2 DefaultScale = new Vector2(0.25f, 0.25f);

        public Texture2D Checked;
        public Texture2D Unchecked;
        public Vector2? Scale = DefaultScale;

        public bool Value = false;
        public Func<SButton, bool> GetValue;
        public Action<SButton, bool> SetValue;

        public override void Init() {
            if (DefaultChecked == null) {
                DefaultChecked = YLModContent.Load<Texture2D>("ylmod/gui/checkbox_checked");
                if (DefaultChecked != null) {
                    DefaultChecked.filterMode = FilterMode.Trilinear;
                }
            }
            if (DefaultUnchecked == null) {
                DefaultUnchecked = YLModContent.Load<Texture2D>("ylmod/gui/checkbox_unchecked");
                if (DefaultUnchecked != null) {
                    DefaultUnchecked.filterMode = FilterMode.Trilinear;
                }
            }

            SButton button = (SButton) Elem;
            button.OnClick += elem => SetValue?.Invoke(button, Value = !Value);
            if (Scale != null)
                button.IconScale = Scale.Value;
        }

        public override void Update() {
            SButton button = (SButton) Elem;
            bool value = GetValue?.Invoke(button) ?? Value;
            Value = value;
            if (value)
                button.Icon = Checked ?? DefaultChecked;
            else
                button.Icon = Unchecked ?? DefaultUnchecked;
        }

    }
}
