using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CreateGameScene
{
    [MenuItem("Tools/Build Survival Shooter Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var manager = new GameObject("Survival Shooter Game").AddComponent<SurvivalShooterGame>();
        var music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Background Music.mp3");
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("backgroundMusic").objectReferenceValue = music;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        const string scenePath = "Assets/Scenes/SurvivalShooterLite.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        Debug.Log("Survival Shooter scene created: " + scenePath);
    }
}
