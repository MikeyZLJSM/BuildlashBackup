using System;
using Unity;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Controllers
{
    [Serializable]
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }
        

        public const string BuildingSceneName = "ModuleBuilding";
        public const string BattleSceneName = "Battle";
        public GameObject modulesManager;
        public GameObject moduleSelector;
        public GameObject uiController;
        public GameObject buildController;
        
        
        public void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            };
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBuildingScene();
        }
        
        public void LoadBuildingScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(BuildingSceneName);
            buildController.SetActive(true);
            moduleSelector.SetActive(true);
        }
        
        public void LoadBattleScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(BattleSceneName);
            buildController.SetActive(true);
            moduleSelector.SetActive(true);
        }
         
        

    }
}