using UnityEngine;

namespace Controllers
{
    public partial class BattleManager
    {
        /// <summary>默认敌人预制体</summary>
        public GameObject defaultEnemyPrefab;

        /// <summary>地面层级名称</summary>
        [Header("地面层级设置")] public string groundLayerMask = "Ground";
        
        /// <summary>敌人生成高度偏移</summary>
        [Header("生成偏移设置")] public float spawnHeightOffset = 0.5f;

        /// <summary>在鼠标点击处生成一个默认的敌人</summary>
        public void GenerateEnemy(GameObject enemyPrefab = null)
        {
            // 检查是否传入了敌人预制体
            if (enemyPrefab is not null)
            {
                defaultEnemyPrefab = enemyPrefab;
            }

            // 获取摄像机实例
            Camera cam = CameraScript.Instance.camera;
    
            // 创建从摄像机到鼠标位置的射线
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            
            // 进行射线投射，只检测指定的地面层级
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(this.groundLayerMask)))
            {
                // 在击中点上方生成敌人，避免生成在地面下
                Vector3 spawnPosition = hit.point + Vector3.up * spawnHeightOffset;
                GameObject enemyObj = Instantiate(defaultEnemyPrefab, spawnPosition, Quaternion.identity);

                // 确保敌人正确朝向中心模块
                if (ModulesManager.Instance?.GetCenterModule() != null)
                {
                    enemyObj.transform.LookAt(ModulesManager.Instance.GetCenterModule().transform);
                }

                Debug.Log($"在Ground层的位置 {hit.point} 生成了敌人");
            }
            else
            {
                Debug.Log("射线未击中Ground层");
            }

        }
    }
}