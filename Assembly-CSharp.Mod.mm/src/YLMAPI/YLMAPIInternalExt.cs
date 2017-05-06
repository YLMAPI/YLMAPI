using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

static class YLMAPIInternalExt {

    public static string GetPath(this Transform t) {
        string path = t.name;
        Transform parent = t;
        while ((parent = parent.parent) != null)
            path = parent.name + "/" + path;
        return path;
    }

    public static StringBuilder AppendIndentation(this StringBuilder builder, int depth, string str = "    ") {
        while (depth-- > 0)
            builder.Append(str);
        return builder;
    }

    public static void LogDetailed(this Exception e, string tag = null) {
        for (Exception e_ = e; e_ != null; e_ = e_.InnerException) {
            Console.WriteLine(e_.GetType().FullName + ": " + e_.Message + "\n" + e_.StackTrace);
            if (e_ is ReflectionTypeLoadException) {
                ReflectionTypeLoadException rtle = (ReflectionTypeLoadException) e_;
                for (int i = 0; i < rtle.Types.Length; i++) {
                    Console.WriteLine("ReflectionTypeLoadException.Types[" + i + "]: " + rtle.Types[i]);
                }
                for (int i = 0; i < rtle.LoaderExceptions.Length; i++) {
                    LogDetailed(rtle.LoaderExceptions[i], tag + (tag == null ? "" : ", ") + "rtle:" + i);
                }
            }
            if (e_ is TypeLoadException) {
                Console.WriteLine("TypeLoadException.TypeName: " + ((TypeLoadException) e_).TypeName);
            }
            if (e_ is BadImageFormatException) {
                Console.WriteLine("BadImageFormatException.FileName: " + ((BadImageFormatException) e_).FileName);
            }
        }
    }

}
