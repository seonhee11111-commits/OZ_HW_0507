using System.Collections.Generic;
using UnityEngine;

namespace TurretDemo
{
    /// <summary>
    /// Turret가 인식할 수 있는 범용 Enemy 표식 컴포넌트입니다.
    /// 활성 Enemy 목록을 static registry로 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyTarget : MonoBehaviour
    {
        private static readonly List<EnemyTarget> ActiveTargetsInternal = new List<EnemyTarget>(64);

        [SerializeField]
        [Tooltip("향후 진영 분리 확장을 위한 팀 식별자입니다.")]
        private int teamId;

        [SerializeField]
        [Tooltip("최대 체력입니다.")]
        private float maxHealth = 10f;

        [SerializeField]
        [Tooltip("사망 위치 표시용 marker를 잠깐 남길지 여부입니다.")]
        private bool showDeathMarker = true;

        [SerializeField]
        [Tooltip("사망 marker 유지 시간(초).")]
        private float deathMarkerLifeTimeSeconds = 0.35f;

        [SerializeField]
        [Tooltip("비워두면 transform.position을 조준점으로 사용합니다.")]
        private Transform aimPoint;

        private float currentHealth;

        private EnemySpawner spawner;

        public static IReadOnlyList<EnemyTarget> ActiveTargets => ActiveTargetsInternal;

        public int TeamId => teamId;

        public float CurrentHealth => currentHealth;
        public Transform CachedTransform => transform;

        public Vector3 AimWorldPosition => aimPoint != null ? aimPoint.position : transform.position;

        private bool isDead;

        [SerializeField] private float ShakeDuration = 0.5f;

        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void Initialize(EnemySpawner spawner)
        {
            this.spawner = spawner;
        }

        private void OnEnable()
        {
            currentHealth = maxHealth;
            isDead = false;

            if (!ActiveTargetsInternal.Contains(this))
            {
                ActiveTargetsInternal.Add(this);
            }

            StartCoroutine(SpawnScaleRoutine());
        }

        private void OnDisable()
        {
            ActiveTargetsInternal.Remove(this);
        }

        /// <summary>
        /// 피격 데미지를 적용하고 체력이 0 이하가 되면 Enemy를 제거합니다.
        /// </summary>
        /// <param name="damageAmount">적용할 데미지량.</param>
        /// <returns>데미지가 적용되면 true를 반환합니다.</returns>
        public bool ApplyDamage(float damageAmount)
        {
            if (damageAmount <= 0f || isDead)
            {
                return false;
            }

            currentHealth -= damageAmount;

            if (currentHealth <= 0f)
            {
                isDead = true;
                CriticalHitController.Instance.StartFlash(0.1f, 0.8f);
                StartCoroutine(DieShakeCoroutine());
                //Die();
            }

            return true;
        }

        private void Die()
        {
            if (showDeathMarker)
            {
                CreateDeathMarker();
            }

            //Destroy(gameObject);
            isDead = false;
            spawner.ReturnToPool(gameObject);
        }

        private System.Collections.IEnumerator DieShakeCoroutine()
        {
            float elapsed = 0f;
            Vector3 originalPos = transform.position;

            while (elapsed < ShakeDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = originalPos + UnityEngine.Random.insideUnitSphere * 0.1f;
                yield return null;
            }

            Die();
        }

        private System.Collections.IEnumerator SpawnScaleRoutine()
        {
            float duration = 0.5f;
            float elapsed = 0f;

            transform.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                yield return null;
            }
            transform.localScale  = originalScale;
        }


        private void CreateDeathMarker()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "EnemyDeathMarker";
            marker.transform.position = AimWorldPosition;
            marker.transform.localScale = Vector3.one * 0.45f;

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }

            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                // 사망 지점을 눈에 띄게 보여주기 위한 임시 색상입니다.
                markerRenderer.material.color = new Color(1f, 0.2f, 0.2f, 1f);
            }

            Destroy(marker, deathMarkerLifeTimeSeconds);
        }
    }
}
