using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Debug.Log( "Scene loaded: " + scene.name );
        
        var path = "Variants";
        var collection = Resources.Load<ShaderVariantCollection>( path );
        
        if ( collection != null )
        {
            Debug.Log( "Shaders/variants: " + collection.shaderCount + "/" + collection.variantCount );
        
            collection.WarmUp();
        
            Resources.UnloadAsset( collection );
        }
    }
}
