using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;

namespace TurretDemo
{
    /// <summary>
    /// 랜덤 SpawnPoint에서 Enemy를 생성하고 최대 개수를 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject enemyPrefab;

        [SerializeField]
        [Tooltip("랜덤 생성 위치 목록.")]
        private Transform[] spawnPoints;

        [SerializeField]
        [Tooltip("생성된 Enemy의 부모(선택).")]
        private Transform enemyRoot;

        [Header("Spawn Settings")]
        [SerializeField]
        [Min(1)]
        private int initialSpawnCount = 4;

        [SerializeField]
        [Min(1)]
        private int maxAliveCount = 8;

        [SerializeField]
        [Min(0.1f)]
        private float spawnIntervalSeconds = 1f;

        [SerializeField]
        [Tooltip("생성 시 Turret 중앙점을 향하도록 forward를 배치합니다.")]
        private Transform lookAtCenter;

        [SerializeField]
        [Min(0.1f)]
        private float enemyMoveSpeedUnitsPerSecond = 5f;

        [SerializeField]
        [Min(0.1f)]
        private float enemyLifeTimeSeconds = 10f;

        private readonly List<GameObject> aliveEnemies = new List<GameObject>(32);
        private float nextSpawnTimeSeconds;

        private ObjectPool<GameObject> Pool;
        [SerializeField] private GameObject Prefab;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 100;



        private void Awake()
        {
            Pool = new ObjectPool<GameObject>
            (
                createFunc: CreateEnemy,
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                actionOnDestroy: OnDestroyEnemy,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private GameObject CreateEnemy()
        {
            GameObject obj = Instantiate(Prefab);
            obj.name = $"Pooled_{Prefab.name}";
            obj.SetActive(false);
            return obj;
        }

        private void OnGetEnemy(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnReleaseEnemy(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void OnDestroyEnemy(GameObject obj)
        {
            Destroy(obj);
        }


        private void Start()
        {
            for (int spawnIndex = 0; spawnIndex < initialSpawnCount; spawnIndex++)
            {
                if (!TrySpawnOneEnemy())
                {
                    break;
                }
            }
        }

        private void Update()
        {
            RemoveDestroyedEntries();
            if (Time.time < nextSpawnTimeSeconds)
            {
                return;
            }

            if (aliveEnemies.Count >= maxAliveCount)
            {
                return;
            }

            if (TrySpawnOneEnemy())
            {
                nextSpawnTimeSeconds = Time.time + spawnIntervalSeconds;
            }
        }

        private bool TrySpawnOneEnemy()
        {
            if (Prefab == null || spawnPoints == null || spawnPoints.Length == 0)
            {
                return false;
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (spawnPoint == null)
            {
                return false;
            }

            Quaternion rotation = spawnPoint.rotation;
            if (lookAtCenter != null)
            {
                Vector3 toCenter = lookAtCenter.position - spawnPoint.position;
                if (toCenter.sqrMagnitude > 1e-8f)
                {
                    rotation = Quaternion.LookRotation(toCenter.normalized, Vector3.up);
                }
            }

            //GameObject spawned = Instantiate(enemyPrefab, spawnPoint.position, rotation, enemyRoot);
            GameObject spawned = Pool.Get();
            spawned.transform.SetPositionAndRotation(spawnPoint.position, rotation);
            spawned.transform.SetParent(enemyRoot);

            EnemyLinearMover mover = spawned.GetComponent<EnemyLinearMover>();
            if (spawned.TryGetComponent(out EnemyLinearMover Enemymover))
            {
                Enemymover.Initialize(this, enemyMoveSpeedUnitsPerSecond, enemyLifeTimeSeconds);
            }

            if (spawned.TryGetComponent(out EnemyTarget target))
            {
                target.Initialize(this);
            }

            aliveEnemies.Add(spawned);
            return true;
        }

        private void RemoveDestroyedEntries()
        {
            for (int index = aliveEnemies.Count - 1; index >= 0; index--)
            {
                GameObject enemy = aliveEnemies[index];
                if (enemy == null || !enemy.activeInHierarchy)
                {
                    if (enemy != null && enemy.activeInHierarchy)
                    {
                        Pool.Release(enemy);
                    }
                    aliveEnemies.RemoveAt(index);
                }
            }
        }

        public void ReturnToPool(GameObject enemy)
        {
            Pool.Release(enemy);
        }

    }
}
