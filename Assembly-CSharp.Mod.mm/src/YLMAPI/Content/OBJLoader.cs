﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using SGUI;
using Rewired;
using UEInput = UnityEngine.Input;
using System.IO;
using System.Reflection;
using MonoMod.Detour;

namespace YLMAPI.Content.OBJ {
    public static class OBJParser {

        private static Vector2 _ParseV2(string[] data, int offs)
            => new Vector2(
                float.Parse(data[offs + 0]),
                float.Parse(data[offs + 1])
            );

        private static Vector3 _ParseV3(string[] data, int offs)
            => new Vector3(
                float.Parse(data[offs + 0]),
                float.Parse(data[offs + 1]),
                float.Parse(data[offs + 2])
            );
        
        private static bool _Count(string str, char c, int count) {
            for (int i = 0; i < str.Length; i++) {
                if (str[i] == c) {
                    count--;
                    if (count < 0)
                        return false;
                }
            }
            return count == 0;
        }

        public static OBJData ParseOBJ(Stream stream, OBJParserStatus s = null) {
            using (StreamReader reader = new StreamReader(stream))
                return ParseOBJ(reader, s);
        }
        public static OBJData ParseOBJ(StreamReader reader, OBJParserStatus s = null) {
            if (s == null)
                s = new OBJParserStatus();

            OBJObject o;

            // Parsing
            while (!reader.EndOfStream) {
                string line = reader.ReadLine().Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line[0] == '#' && (line.Length < 2 || line[1] != '!'))
                    continue;

                string[] data = line.Split(' ');

                switch (data[0]) {
                    case "mtllib":
                        // Currently ignore material libraries.
                        break;

                    case "o":
                        o = new OBJObject(line.Substring(3), s.Current);
                        s.Data.Objects.Add(o);
                        s.Current = o.Groups[0];
                        s.CurrentMaterial = "";
                        break;

                    case "g":
                        if (s.Data.Objects.Count == 0) {
                            o = new OBJObject("", s.Current);
                            s.Current = o;
                            s.Current = o.Groups[0];
                            s.Current.Name = line.Substring(3);
                        } else {
                            s.Current = new OBJGroup(line.Substring(3), s.Current);
                            if (s.Current.PrevObj.Groups.Count == 1 &&
                                s.Current.PrevObj.Groups[0].Name == "") {
                                s.Current.PrevObj.Groups.RemoveAt(0);
                            }
                        }
                        s.CurrentMaterial = "";
                        break;

                    case "v":
                        s.Current.Vertices.Add(_ParseV3(data, 1));
                        break;

                    case "vt":
                        s.Current.UVs.Add(_ParseV2(data, 1));
                        break;

                    case "vn":
                        s.Current.Normals.Add(_ParseV3(data, 1));
                        break;

                    case "usemtl":
                        // Currently ignore material usages.
                        s.Current.Materials.Add(line.Substring(8));
                        break;

                    case "s":
                        if (data[1] == "off") {
                            // Currently ignore smoothing group disabling.
                        } else {
                            // Currently ignore smoothing group 1 - 32.
                        }
                        break;

                    case "f":
                        OBJGroup current = s.Current;
                        if (4 <= data.Length && data.Length < 6) {
                            int[] indices = new int[data.Length - 1];
                            for (int i = 0; i < indices.Length; i++) {
                                string elem = data[i + 1];

                                int iV = -1;
                                int iN = -1;
                                int iUV = -1;

                                if (elem.Contains("//")) {
                                    string[] parts = elem.Split('/');
                                    iV = int.Parse(parts[0]) - 1;
                                    iN = int.Parse(parts[2]) - 1;

                                } else if (_Count(elem, '/', 2)) {
                                    string[] parts = elem.Split('/');
                                    iV = int.Parse(parts[0]) - 1;
                                    iUV = int.Parse(parts[1]) - 1;
                                    iN = int.Parse(parts[2]) - 1;

                                } else if (!elem.Contains("/")) {
                                    iV = int.Parse(elem) - 1;

                                } else {
                                    string[] parts = elem.Split('/');
                                    iV = int.Parse(parts[0]) - 1;
                                    iUV = int.Parse(parts[1]) - 1;
                                }

                                string cacheKey = $"{iV}; {iN}; {iUV}";
                                int cacheValue;
                                if (current.IndexCache.TryGetValue(cacheKey, out cacheValue))
                                    indices[i] = cacheValue;
                                else {
                                    cacheValue = current.IndexCache.Count;
                                    indices[i] = cacheValue;
                                    current.IndexCache[cacheKey] = cacheValue;
                                    current.UVertices.Add(current.Vertices[iV]);

                                    if (iN < 0 || current.Normals.Count <= iN)
                                        current.UNormals.Add(Vector3.zero);
                                    else {
                                        current.ContainsNormals = true;
                                        current.UNormals.Add(current.Normals[iN]);
                                    }

                                    if (iUV < 0 || current.UVs.Count <= iUV)
                                        current.UUVs.Add(Vector2.zero);
                                    else
                                        current.UUVs.Add(current.UVs[iUV]);
                                }
                            }

                            OBJFace face = new OBJFace();
                            face.IndexMap = OBJFace.DefaultIndexMap;
                            face.RawIndices = indices;
                            face.Material = s.CurrentMaterial;
                            s.Current.AddFace(face);
                            if (indices.Length > 3) {
                                face = new OBJFace();
                                face.IndexMap = OBJFace.SecondaryIndexMap;
                                face.RawIndices = indices;
                                face.Material = s.CurrentMaterial;
                                s.Current.AddFace(face);
                            }
                        }

                        break;

                        // TODO: [OBJLoader] Interpret custom data (prefix #!).
                }


            }

            return s.Data;
        }

    }

    public class OBJParserStatus {

        public OBJData Data = new OBJData();

        public OBJGroup Current;
        public string CurrentMaterial;

    }

    public class OBJData {

        public List<string> MaterialLibraries = new List<string>();
        public List<OBJObject> Objects = new List<OBJObject>();

        public List<Mesh> ToMeshes() {
            List<Mesh> meshes = new List<Mesh>(Objects.Count);
            for (int i = 0; i < Objects.Count; i++)
                meshes.AddRange(Objects[i].ToMeshes());
            return meshes;
        }

    }

    public class OBJGroup {

        public string Name;

        public OBJGroup Prev;
        public OBJObject PrevObj;

        public List<Vector3> Vertices = new List<Vector3>();
        public List<Vector2> UVs = new List<Vector2>();
        public List<Vector3> Normals = new List<Vector3>();

        public Dictionary<string, int> IndexCache = new Dictionary<string, int>();

        public bool ContainsNormals = false;

        public List<Vector3> UVertices = new List<Vector3>();
        public List<Vector3> UNormals = new List<Vector3>();
        public List<Vector2> UUVs = new List<Vector2>();

        public List<string> Materials = new List<string>();
        public Dictionary<string, List<OBJFace>> Faces = new Dictionary<string, List<OBJFace>>();

        public OBJGroup() {
        }
        public OBJGroup(string name, OBJGroup prev)
            : this() {
            Name = name;
            Prev = prev;

            PrevObj = prev as OBJObject ?? prev?.PrevObj;
            if (PrevObj != null && !(this is OBJObject))
                PrevObj.Groups.Add(this);
        }

        public void AddFace(OBJFace face) {
            List<OBJFace> faces;
            if (!Faces.TryGetValue(face.Material, out faces))
                Faces[face.Material] = faces = new List<OBJFace>();
            faces.Add(face);
        }

        public virtual Mesh ToMesh() {
            Mesh mesh = new Mesh();
            mesh.name = Name;

            List<Vector3> pVertices = new List<Vector3>();
            List<Vector3> pNormals = new List<Vector3>();
            List<Vector2> pUVs = new List<Vector2>();
            List<List<int>> pIndices = new List<List<int>>();
            Dictionary<int, int> map = new Dictionary<int, int>();

            HashSet<string> materials = new HashSet<string>();

            foreach (KeyValuePair<string, List<OBJFace>> facesPerMat in Faces) {
                string mat = facesPerMat.Key;
                List<OBJFace> faces = facesPerMat.Value;
                if (faces.Count == 0)
                    continue;

                List<int> indices = new List<int>();
                for (int fi = 0; fi < faces.Count; fi++) {
                    OBJFace face = faces[fi];
                    indices.Add(face[0]);
                    indices.Add(face[1]);
                    indices.Add(face[2]);
                }

                if (!materials.Contains(mat)) {
                    materials.Add(mat);
                    mesh.subMeshCount++;
                }

                for (int ii = 0; ii < indices.Count; ii++) {
                    int index = indices[ii];
                    if (map.ContainsKey(index))
                        indices[ii] = map[index];
                    else {
                        indices[ii] = map[index] = pVertices.Count;
                        pVertices.Add(UVertices[index]);
                        pNormals.Add(UNormals[index]);
                        pUVs.Add(UUVs[index]);
                    }
                }

                pIndices.Add(indices);
            }

            mesh.SetVertices(pVertices);
            mesh.SetNormals(pNormals);
            mesh.SetUVs(0, pUVs);

            for (int i = 0; i < pIndices.Count; i++)
                mesh.SetTriangles(pIndices[i], i);

            if (!ContainsNormals)
                mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.Optimize();

            return mesh;
        }

    }

    public class OBJObject : OBJGroup {

        public List<OBJGroup> Groups = new List<OBJGroup>();

        public OBJObject()
            : base() {
        }
        public OBJObject(string name, OBJGroup prev)
            : base(name, prev) {
            new OBJGroup("", this);
        }

        public override Mesh ToMesh() {
            if (Groups.Count == 0)
                return null;
            if (Groups.Count == 1)
                return Groups[0].ToMesh();

            Mesh mesh = new Mesh();
            mesh.name = Name;
            CombineInstance[] cis = new CombineInstance[Groups.Count];
            for (int i = 0; i < Groups.Count; i++)
                cis[i] = new CombineInstance() {
                    mesh = Groups[i].ToMesh()
                };
            mesh.CombineMeshes(cis);
            return mesh;
        }

        public List<Mesh> ToMeshes() {
            List<Mesh> meshes = new List<Mesh>(Groups.Count);
            for (int i = 0; i < Groups.Count; i++)
                meshes.Add(Groups[i].ToMesh());
            return meshes;
        }

    }

        public class OBJFace {

        public readonly static int[] DefaultIndexMap = { 0, 1, 2 };
        public readonly static int[] SecondaryIndexMap = { 2, 3, 0 };

        public string Material;
        public int[] RawIndices;

        public int[] IndexMap = DefaultIndexMap;

        public int this[int i] {
            get {
                return RawIndices[IndexMap[i]];
            }
            set {
                RawIndices[IndexMap[i]] = value;
            }
        }

        public int Length => IndexMap.Length;

    }

}
