using System;
using System.Text;
using UnityEngine;

namespace SGUI {
    public class SCheckboxModifier : SModifier {

        public static Texture2D DefaultChecked = YLMod.Content.Load<Texture2D>("ylmod/gui/checkbox_checked");
        public static Texture2D DefaultUnchecked = YLMod.Content.Load<Texture2D>("ylmod/gui/checkbox_unchecked");
        public static Vector2 DefaultScale = new Vector2(0.25f, 0.25f);

        static SCheckboxModifier() {
            DefaultChecked.filterMode = FilterMode.Trilinear;
            DefaultUnchecked.filterMode = FilterMode.Trilinear;
        }

        public Texture2D Checked;
        public Texture2D Unchecked;
        public Vector2? Scale = DefaultScale;

        public bool Value = false;
        public Func<SButton, bool> GetValue;
        public Action<SButton, bool> SetValue;

        public override void Init() {
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
                button.Icon = Checked ?? DefaultUnchecked;
            else
                button.Icon = Unchecked ?? DefaultChecked;
        }

    }
}
