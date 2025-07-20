
using UnityEngine;
using UnityEngine.Serialization;

namespace Controllers

{
    /// <summary>管理ui相关逻辑</summary>
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }
        public void Awake()
        {
            if (Instance is not null) return;
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        
        public void EnterBattleOnclick()
        {
            Controllers.SceneManager.Instance.LoadBattleScene();
            print("进入战斗场景");
        }
        
       

    }
}

