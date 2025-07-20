using TMPro;
using UnityEngine;

namespace Controllers
{
    /// <summary>管理ui相关逻辑</summary>
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }
        private float _currHealth;
        [Header("UI引用")]
        public GameObject enterBattleButton;
        public GameObject enterBuildingButton;
        public TMP_Text healthText;
        
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            };
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        
        /// <summary>启用时订阅场景切换事件</summary>
        private void OnEnable()
        {
            Controllers.SceneManager.OnSceneChanged += OnSceneChanged;
        }

        /// <summary>禁用时取消订阅场景切换事件</summary>
        private void OnDisable()
        {
            Controllers.SceneManager.OnSceneChanged -= OnSceneChanged;
        }

        /// <summary>场景切换回调，根据场景名称切换按钮显示</summary>
        private void OnSceneChanged(string sceneName)
        {
            switch (sceneName)
            {
                case Controllers.SceneManager.BuildingSceneName:
                    enterBattleButton.SetActive(true);
                    enterBuildingButton.SetActive(false);
                    healthText.gameObject.SetActive(false);
                    break;
                case Controllers.SceneManager.BattleSceneName:
                    enterBattleButton.SetActive(false);
                    enterBuildingButton.SetActive(true);
                    healthText.gameObject.SetActive(true);
                    break;
            }
        }
        
        public void EnterBattleOnclick()
        {
            Controllers.SceneManager.Instance.LoadBattleScene();
            print("进入战斗场景");
        }
        
        public void EnterBuildingOnclick()
        {
            Controllers.SceneManager.Instance.LoadBuildingScene();
            print("进入建筑场景");
        }
        /// <summary>更新血条显示</summary>
        public void UpdateHealthText(float currentHealth, float maxHealth)
        {
            
            _currHealth = currentHealth;
            healthText.text = currentHealth + "/" + maxHealth;
            
        }
    }
}
