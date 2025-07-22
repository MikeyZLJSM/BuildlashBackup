using System;
using UnityEngine;

namespace Controllers
{
    [Serializable]
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }
        
        /// <summary>场景切换事件，参数为新场景名称</summary>
        public static event System.Action<string> OnSceneChanged;
        
        [Header("场景名称")]
        public const string BuildingSceneName = "ModuleBuilding";
        public const string BattleSceneName = "Battle";
        
        [Header("管理器引用")]
        public GameObject modulesManager;
        public GameObject moduleSelector;
        public GameObject battleManager;
        public GameObject buildController;
        public GameObject bulletManager;
        
        [Header("当前场景名称")]
        public string currentScene;
        
        public void Awake()
        {  
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            
        }
        public void Start()
        {
           
        }
        public void LoadBuildingScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(BuildingSceneName);
            buildController.SetActive(true);
            moduleSelector.SetActive(true);
            battleManager.SetActive(false);
            bulletManager.SetActive(false);
            currentScene = BuildingSceneName;
            OnSceneChanged?.Invoke(BuildingSceneName);
        }
        
        public void LoadBattleScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(BattleSceneName);
            buildController.SetActive(false);
            moduleSelector.SetActive(false);
            battleManager.SetActive(true);
            bulletManager.SetActive(true);
            currentScene = BattleSceneName;
            OnSceneChanged?.Invoke(BattleSceneName);
        }

    }
}