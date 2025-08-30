using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Nat
{
    [CreateAssetMenu(menuName = "Flags/Bool Flag")]
    public class BoolFlag : ScriptableObject
    {
        public bool Value;
        [SerializeField] private bool defaultValue = false;

        private static readonly List<BoolFlag> allFlags = new();

        private void OnEnable()
        {
            if (!allFlags.Contains(this))
                allFlags.Add(this);
        }

        private void OnDisable()
        {
            allFlags.Remove(this);
        }

        public static void ResetAllFlags()
        {
            foreach (var flag in allFlags)
                flag.Value = flag.defaultValue;

            Debug.Log("BoolFlags reset on scene load.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AutoResetFlags()
        {
            ResetAllFlags(); // reset at app start too
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded += (_, _) => ResetAllFlags();
        }
    }
}