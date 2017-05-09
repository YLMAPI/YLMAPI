using UnityEngine;
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

namespace YLMAPI.Content {
    public static class ModContentExt {

        public static bool IsReadable(this Texture2D texture) {
            // return texture.GetRawTextureData().Length != 0; // spams log
            try {
                texture.GetPixels();
                return true;
            } catch {
                return false;
            }
        }

        public static Texture2D Copy(this Texture2D texture, TextureFormat? format = TextureFormat.ARGB32) {
            if (texture == null)
                return null;

            RenderTexture copyRT = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );

            Graphics.Blit(texture, copyRT);

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = copyRT;

            Texture2D copy = new Texture2D(texture.width, texture.height, format != null ? format.Value : texture.format, 1 < texture.mipmapCount);
            copy.name = texture.name;
            copy.ReadPixels(new Rect(0, 0, copyRT.width, copyRT.height), 0, 0);
            copy.Apply(true, false);

            RenderTexture.active = previousRT;
            RenderTexture.ReleaseTemporary(copyRT);

            return copy;
        }

        public static Texture2D Patch(this Texture2D texture, Texture2D patch) {
            if (texture == null)
                return null;
            if (patch == null)
                return texture;

            RenderTexture patchRT = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );

            Graphics.Blit(texture, patchRT);

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = patchRT;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, 1f, 1f, 0f);

            Graphics.DrawTexture(new Rect(0, 0, 1f, 1f), patch);

            GL.PopMatrix();

            texture.ReadPixels(new Rect(0, 0, patchRT.width, patchRT.height), 0, 0);
            texture.Apply(true, false);

            RenderTexture.active = previousRT;
            RenderTexture.ReleaseTemporary(patchRT);

            return texture;
        }

        public static Texture2D Patch(this Texture2D texture, params Texture2D[] patches) {
            if (texture == null)
                return null;
            if (patches == null || patches.Length == 0)
                return texture;

            RenderTexture patchRT = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );

            Graphics.Blit(texture, patchRT);

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = patchRT;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, 1f, 1f, 0f);

            for (int i = 0; i < patches.Length; i++)
                Graphics.DrawTexture(new Rect(0, 0, 1f, 1f), patches[i]);

            GL.PopMatrix();

            texture.ReadPixels(new Rect(0, 0, patchRT.width, patchRT.height), 0, 0);
            texture.Apply(true, false);

            RenderTexture.active = previousRT;
            RenderTexture.ReleaseTemporary(patchRT);

            return texture;
        }

        public static Texture2D Patch(this Texture2D texture, List<Texture2D> patches) {
            if (texture == null)
                return null;
            if (patches == null || patches.Count == 0)
                return texture;

            RenderTexture patchRT = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );

            Graphics.Blit(texture, patchRT);

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = patchRT;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, 1f, 1f, 0f);

            for (int i = 0; i < patches.Count; i++)
                Graphics.DrawTexture(new Rect(0, 0, 1f, 1f), patches[i]);

            GL.PopMatrix();

            texture.ReadPixels(new Rect(0, 0, patchRT.width, patchRT.height), 0, 0);
            texture.Apply(true, false);

            RenderTexture.active = previousRT;
            RenderTexture.ReleaseTemporary(patchRT);

            return texture;
        }

        public static Texture2D Patch(this Texture2D texture, List<AssetMetadata> patches) {
            if (texture == null)
                return null;
            if (patches == null || patches.Count == 0)
                return texture;

            RenderTexture patchRT = RenderTexture.GetTemporary(
                texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );

            Graphics.Blit(texture, patchRT);

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = patchRT;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, 1f, 1f, 0f);

            Texture2D tex = new Texture2D(2, 2);
            for (int i = 0; i < patches.Count; i++) {
                tex.LoadImage(patches[i].Data);
                Graphics.DrawTexture(new Rect(0, 0, 1f, 1f), tex);
            }
            UnityEngine.Object.Destroy(tex);

            GL.PopMatrix();

            texture.ReadPixels(new Rect(0, 0, patchRT.width, patchRT.height), 0, 0);
            texture.Apply(true, false);

            RenderTexture.active = previousRT;
            RenderTexture.ReleaseTemporary(patchRT);

            return texture;
        }

        public static Texture2D GetRW(this Texture2D texture) {
            if (texture == null)
                return null;
            if (texture.IsReadable())
                return texture;
            return texture.Copy();
        }

    }
}
