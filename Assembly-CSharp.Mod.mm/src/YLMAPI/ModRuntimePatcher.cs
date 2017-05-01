#if !MMHARMONY
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
using System.Linq;
using Ionic.Zip;
using MonoMod.InlineRT;
using ReflectionHelper = MonoMod.InlineRT.ReflectionHelper;
using System.Security.Cryptography;
using MonoMod;
using Mono.Cecil;
using MonoMod.Detour;
using System.Runtime.CompilerServices;

namespace YLMAPI {
    internal delegate void d_PrintA();
    internal delegate void d_PrintQDTO(QuickDebugTestObject qdto);
    internal class QuickDebugTestObject {
        public int Value;
        public override string ToString()
            => $"{{QuickDebugTestObject:{Value}}}";
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void PrintQDTO() => Console.WriteLine("QDTO");
    }
    public static class ModRuntimePatcher {

        public static MonoModDetourer Detourer;

        public static void Init() {
            try {
                // TestRuntimeDetourHelper();

                Detourer = new MonoModDetourer() {
                    InputPath = Assembly.GetExecutingAssembly().Location,
                    CleanupEnabled = false,
                    DependencyDirs = {
                        ModRelinker.ManagedDirectory
                    }
                };

                Detourer.ReaderParameters.ReadSymbols = false;

                // DON'T. The assembly is already patched with the .mm.dlls in there!
                // Otherwise this code here wouldn't even run...
                // Modder.ReadMod(ModRelinker.ManagedDirectory);

                Detourer.Read();
                Detourer.MapDependencies();
            } catch (Exception e) {
                ModLogger.Log("rtpatcher", $"Failed initializing: {e}");
                return;
            }
        }

        public static void LoadPatch(Stream stream) {
            try {
                ModLogger.Log("rtpatcher", "Loading new patch");
                Detourer.ReadMod(stream);
                ModRelinker.AssemblyRelinkMap[Detourer.Mods[Detourer.Mods.Count - 1].Assembly.Name.Name] = ModRelinker.AssemblyRelinkedCache["Assembly-CSharp"];
                ModLogger.Log("rtpatcher", $"Applied new patch {Detourer.Mods[Detourer.Mods.Count - 1].Assembly.Name.Name}");
            } catch (Exception e) {
                ModLogger.Log("rtpatcher", $"Failed patching: {e}");
                return;
            }
        }

        internal static bool TestRuntimeDetourHelper() {
            MethodInfo m_PrintA = typeof(ModRuntimePatcher).GetMethod("PrintA", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo m_PrintB = typeof(ModRuntimePatcher).GetMethod("PrintB", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo m_PrintC = typeof(ModRuntimePatcher).GetMethod("PrintC", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo m_PrintATrampoline = typeof(ModRuntimePatcher).GetMethod("PrintATrampoline", BindingFlags.NonPublic | BindingFlags.Static);

            PrintA();
            // A

            d_PrintA t_FromB = m_PrintA.Detour<d_PrintA>(m_PrintB);
            PrintA();
            // B

            t_FromB();
            // A

            unsafe {
                m_PrintATrampoline.Detour(
                    RuntimeDetour.CreateTrampoline(m_PrintA)
                );
                PrintATrampoline();
                // A
            }

            d_PrintA t_FromC = m_PrintA.Detour<d_PrintA>((Action) PrintC);
            PrintA();
            // C

            t_FromC();
            // B

            m_PrintA.GetOrigTrampoline<d_PrintA>()();
            // A

            m_PrintB.Detour(m_PrintC);
            PrintB();
            // C

            m_PrintB.Detour((Action) PrintD);
            PrintB();
            // D

            m_PrintA.Undetour();
            m_PrintA.Undetour();
            PrintA();
            // A


            MethodInfo m_PrintQDTO = typeof(QuickDebugTestObject).GetMethod("PrintQDTO", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo m_PrintQDTODetour = typeof(ModRuntimePatcher).GetMethod("PrintQDTODetour", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo m_PrintQDTOTrampoline = typeof(ModRuntimePatcher).GetMethod("PrintQDTOTrampoline", BindingFlags.NonPublic | BindingFlags.Static);

            QuickDebugTestObject qdto = new QuickDebugTestObject();
            qdto.PrintQDTO();
            // QDTO

            d_PrintQDTO t_FromQDTO = m_PrintQDTO.Detour<d_PrintQDTO>(m_PrintQDTODetour);
            qdto.PrintQDTO();
            // QDTO Detour

            t_FromQDTO(qdto);
            // QDTO

            unsafe
            {
                m_PrintQDTOTrampoline.Detour(
                    RuntimeDetour.CreateTrampoline(m_PrintQDTO)
                );
                PrintQDTOTrampoline(qdto);
                // QDTO
            }

            return true;
        }

        // Should only affect .NET Framework
        internal static void PrintA() => Console.WriteLine("A");
        internal static void PrintB() => Console.WriteLine("B");
        internal static void PrintC() => Console.WriteLine("C");
        internal static void PrintD() => Console.WriteLine("D");
        internal static void PrintATrampoline() => Console.WriteLine("SHOULD BE DETOURED");
        internal static void PrintQDTODetour(QuickDebugTestObject qdto) => Console.WriteLine("QDTO Detoured");
        internal static void PrintQDTOTrampoline(QuickDebugTestObject qdto) => Console.WriteLine("SHOULD BE DETOURED");

    }
}
#endif
