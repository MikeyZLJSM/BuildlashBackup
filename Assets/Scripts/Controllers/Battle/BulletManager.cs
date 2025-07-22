using System.Collections.Generic;
using Module.Enums;
using UnityEngine;

namespace Controllers.Battle
{
    /// <summary>
    /// 子弹管理器，负责管理所有子弹的生成、追踪和销毁
    /// </summary>
    public class BulletManager : MonoBehaviour
    {
        public static BulletManager Instance { get; private set; }
        
        [Header("子弹池设置")]
        [SerializeField] private int _initialPoolSize = 20;
        [SerializeField] private bool _expandPoolIfNeeded = true;
        
        [Header("清理设置")]
        [SerializeField] private float _bulletLifetime = 10f; // 子弹最大生命周期
        [SerializeField] private float _cleanupInterval = 5f; // 清理间隔
        
        // 子弹池，按预制体类型分类
        private Dictionary<GameObject, Queue<Bullet>> _bulletPools = new();
        
        // 当前活跃的子弹列表
        private List<Bullet> _activeBullets = new();
        
        private float _lastCleanupTime;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            // 定期清理失效的子弹
            if (Time.time - _lastCleanupTime > _cleanupInterval)
            {
                CleanupInactiveBullets();
                _lastCleanupTime = Time.time;
            }
        }
        
        public Bullet SpawnAndFireBullet(GameObject bulletPrefab, Vector3 spawnPosition, Transform target, 
                                  int damage, DamageType damageType, float speed)
        {
            // 获取或创建子弹
            Bullet bullet = GetBulletFromPool(bulletPrefab);
            
            // 设置子弹位置
            bullet.transform.position = spawnPosition;
            bullet.transform.rotation = Quaternion.identity;
            bullet.gameObject.SetActive(true);
            
            // 初始化子弹属性
            bullet.Initialize(target, damage, damageType, speed, _bulletLifetime);
            
            // 添加到活跃子弹列表
            _activeBullets.Add(bullet);
            
            return bullet;
        }
        
        private Bullet GetBulletFromPool(GameObject bulletPrefab)
        {
            // 如果这种类型的子弹池不存在，创建一个
            if (!_bulletPools.TryGetValue(bulletPrefab, out Queue<Bullet> bulletPool))
            {
                bulletPool = new Queue<Bullet>();
                _bulletPools[bulletPrefab] = bulletPool;
                
                for (int i = 0; i < _initialPoolSize; i++)
                {
                    CreateNewBullet(bulletPrefab, bulletPool);
                }
            }
            
            // 尝试从池中获取子弹
            if (bulletPool.Count > 0)
            {
                return bulletPool.Dequeue();
            }
            
            // 如果池为空且允许扩展，创建新子弹
            if (_expandPoolIfNeeded)
            {
                return CreateNewBullet(bulletPrefab, bulletPool);
            }
            
            // 如果不允许扩展，重用最早的活跃子弹
            Debug.LogWarning("子弹池已空，重用最早的活跃子弹");
            if (_activeBullets.Count > 0)
            {
                Bullet oldestBullet = _activeBullets[0];
                _activeBullets.RemoveAt(0);
                return oldestBullet;
            }
            
            // 如果没有活跃子弹，创建一个新的
            return CreateNewBullet(bulletPrefab, bulletPool);
        }
        
        private Bullet CreateNewBullet(GameObject bulletPrefab, Queue<Bullet> bulletPool)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, transform);
            bulletObj.SetActive(false);
            
            // 确保子弹对象有Bullet组件
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (!bullet)
            {
                bullet = bulletObj.AddComponent<Bullet>();
            }
            
            // 设置回收回调
            bullet.OnDeactivate = () => ReturnBulletToPool(bullet, bulletPrefab);
            
            return bullet;
        }
        
        private void ReturnBulletToPool(Bullet bullet, GameObject bulletPrefab)
        {
            if (!bullet) return;
            
            _activeBullets.Remove(bullet);
            
            bullet.gameObject.SetActive(false);
            
            if (_bulletPools.TryGetValue(bulletPrefab, out Queue<Bullet> bulletPool))
            {
                bulletPool.Enqueue(bullet);
            }
        }
        
        private void CleanupInactiveBullets()
        {
            for (int i = _activeBullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = _activeBullets[i];
                
                // 如果子弹为空或已超时，移除
                if (!bullet || !bullet.gameObject.activeInHierarchy || bullet.IsExpired())
                {
                    if (bullet)
                    {
                        bullet.Deactivate();
                    }
                    else
                    {
                        _activeBullets.RemoveAt(i);
                    }
                }
            }
        }
        
    }
} 