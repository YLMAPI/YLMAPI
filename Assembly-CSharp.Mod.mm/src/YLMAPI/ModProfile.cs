using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YLMAPI {
    public class ModProfile {
        public readonly int Id;
        public readonly string Name;

        public ModProfile(int id, string name) {
            Id = id;
            Name = name;
        }

        public bool RunsOn(ModProfile p) {
            return Id <= p.Id;
        }
        public bool Runs() {
            return RunsOn(ModAPI.Profile);
        }

        public override bool Equals(object obj) {
            ModProfile p = obj as ModProfile;
            if (p == null) {
                return false;
            }
            return p.Id == Id;
        }

        public override int GetHashCode() {
            return Id;
        }

        public static bool operator <(ModProfile a, ModProfile b) {
            if ((a == null) || (b == null)) {
                return false;
            }
            return a.Id < b.Id;
        }
        public static bool operator >(ModProfile a, ModProfile b) {
            if ((a == null) || (b == null)) {
                return false;
            }
            return a.Id > b.Id;
        }

        public static bool operator <=(ModProfile a, ModProfile b) {
            if ((a == null) || (b == null)) {
                return false;
            }
            return a.Id <= b.Id;
        }
        public static bool operator >=(ModProfile a, ModProfile b) {
            if ((a == null) || (b == null)) {
                return false;
            }
            return a.Id >= b.Id;
        }

        public static bool operator ==(ModProfile a, ModProfile b) {
            if ((a == null) || (b == null)) {
                return false;
            }
            return a.Id == b.Id;
        }
        public static bool operator !=(ModProfile a, ModProfile b) {
            return !(a == b);
        }
    }
}
