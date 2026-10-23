using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldForge.EditorTools
{
    /// <summary>
    /// When the project is opened with no saved scene (e.g. a fresh clone), open the WorldForge scene
    /// automatically so pressing Play works straight away.
    /// </summary>
    [InitializeOnLoad]
    static class OpenSceneOnLoad
    {
        const string SessionKey = "WorldForge.OpenedOnLoad";

        static OpenSceneOnLoad()
        {
            if (Application.isBatchMode || SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                    EditorSceneManager.OpenScene(SceneBuilder.ScenePath);
            };
        }
    }
}
