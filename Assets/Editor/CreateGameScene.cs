using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateGameScene
{
    [MenuItem("Tools/Build Orb Collector Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var manager = new GameObject("Orb Collector Game").AddComponent<OrbCollectorGame>();
        var music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Background Music.mp3");
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("backgroundMusic").objectReferenceValue = music;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        const string scenePath = "Assets/Scenes/OrbCollector.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        Debug.Log("Orb Collector scene created: " + scenePath);
    }
}
