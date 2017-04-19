using System;
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

    public static StringBuilder DumpHierarchy(this Scene s, StringBuilder builder, int depth = 0) {
        builder
            .AppendIndentation(depth)
            .AppendFormat("Name: {0}", s.name)
            .AppendLine();

        builder.AppendLine();

        GameObject[] rootObjs = s.GetRootGameObjects();
        builder
            .AppendIndentation(depth)
            .AppendFormat("Root objects: {0}", rootObjs.Length)
            .AppendLine();
        if (rootObjs.Length == 0)
            builder.AppendLine();
        else
            for (int ci = 0; ci < rootObjs.Length; ci++) {
                GameObject c = rootObjs[ci];
                c.transform.DumpHierarchy(builder, depth + 1);
            }

        return builder;
    }

    public static StringBuilder DumpHierarchy(this Transform t, StringBuilder builder, int depth = 0) {
        builder
            .AppendIndentation(depth)
            .AppendFormat("Name: {0}", t.name)
            .AppendLine();
        builder
            .AppendIndentation(depth)
            .AppendFormat("Path: {0}", t.GetPath())
            .AppendLine();

        builder.AppendLine();

        Component[] components = t.GetComponents<Component>();
        builder
            .AppendIndentation(depth)
            .AppendFormat("Components: {0}", components.Length)
            .AppendLine();
        if (components.Length == 0)
            builder.AppendLine();
        else
            for (int ci = 0; ci < components.Length; ci++) {
                Component c = components[ci];
                c.DumpComponent(builder, depth + 1);
            }

        builder
            .AppendIndentation(depth)
            .AppendFormat("Children: {0}", t.childCount)
            .AppendLine();
        if (t.childCount == 0)
            builder.AppendLine();
        else
            for (int ci = 0; ci < t.childCount; ci++) {
                Transform c = t.GetChild(ci);
                c.DumpHierarchy(builder, depth + 1);
            }

        return builder;
    }

    public static StringBuilder DumpComponent(this Component c, StringBuilder builder, int depth = 0) {
        builder
            .AppendIndentation(depth)
            .AppendFormat("Component: {0}", c.GetType())
            .AppendLine();

        builder.AppendLine();

        return builder;
    }

}
