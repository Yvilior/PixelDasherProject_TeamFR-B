using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//Service: Ce script sert a selectionner une scene.
//Objet: Le Canvas
//Auteur: Yvain
//Utilisation: Appelez la fonction SelectScene([index de la Scene dans la SceneList du "BuildProfile"]) via un boutonUI pour charger la scene voulu.
public class SceneSelector : MonoBehaviour
{
    public void SelectScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
