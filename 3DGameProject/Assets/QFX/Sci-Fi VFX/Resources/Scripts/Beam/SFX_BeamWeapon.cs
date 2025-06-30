using UnityEngine;

// ReSharper disable once CheckNamespace
namespace QFX.SFX
{
    public class SFX_BeamWeapon : SFX_ControlledObject
    {
        public AudioClip BeamSound; // Inspector에서 사운드 지정
        private AudioSource _audioSource;
        public Transform StartTransform;
        public float damagePerSecond = 10f; // Inspector에서 조절
        private float damageTimer = 0f;
        [HideInInspector] public Vector3 EndPosition;

        public GameObject LaunchParticleSystem;
        public Light LaunchLight;

        public GameObject ImpactParticleSystem;
        public float ImpactOffset;

        public LineRenderer LineRenderer;

        public float MaxDistance;

        public float LightIntensity;
        public float LineRendererWidth;
        public float AppearSpeed;

        private float _appearProgress;
        private float _beamActivatedTime;

        private ParticleSystem _launchPs;
        private ParticleSystem _impactPs;

        private LineRenderer _lineRenderer;

        public override void Setup()
        {
            base.Setup();

            _lineRenderer = Instantiate(LineRenderer, transform, true);

            _lineRenderer.positionCount = 2;
            LaunchLight.intensity = 0;
            _lineRenderer.widthMultiplier = 0;

            var launchGo = Instantiate(LaunchParticleSystem, StartTransform.position, Quaternion.identity,
                StartTransform);

            _launchPs = launchGo.GetComponent<ParticleSystem>();
            _impactPs = Instantiate(ImpactParticleSystem).GetComponent<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true; // 빔 사운드는 루프 재생이 일반적
        }

        public override void Run()
        {
            if (IsRunning)
                return;

            _launchPs.Play();
            _impactPs.Play();
            // 빔 사운드 재생
            if (BeamSound != null && _audioSource != null)
            {
                _audioSource.clip = BeamSound;
                _audioSource.Play();
            }
            base.Run();
        }

        public override void Stop()
        {
            base.Stop();

            _launchPs.Stop();
            _impactPs.Stop();

            // 빔 사운드 정지
            if (_audioSource != null && _audioSource.isPlaying)
                _audioSource.Stop();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                Run();
            else if (Input.GetMouseButtonUp(0))
                Stop();

            if (!IsRunning && _appearProgress <= 0.0f)
                return;

            var incValue = Time.deltaTime * AppearSpeed;

            if (IsRunning)
                _appearProgress += incValue;
            else
                _appearProgress -= incValue;

            _appearProgress = Mathf.Clamp01(_appearProgress);

            RaycastHit hit;
            bool hitSomething = Physics.Raycast(transform.position, transform.forward, out hit, MaxDistance);
            if (hitSomething)
                EndPosition = hit.point;
            else EndPosition = transform.position + transform.forward * MaxDistance;

            // === 데미지 처리 ===
            if (IsRunning && hitSomething)
            {
                AlienAI alien = hit.collider.GetComponent<AlienAI>();
                if (alien != null)
                {
                    damageTimer += Time.deltaTime;
                    if (damageTimer >= 1f)
                    {
                        alien.TakeDamage((int)damagePerSecond);
                        damageTimer = 0f;
                    }
                }
                else
                {
                    damageTimer = 0f;
                }
            }
            else
            {
                damageTimer = 0f;
            }
            // ===================

            var hitPosition = hitSomething ? hit.point : transform.position + transform.forward * MaxDistance;

            var startPosition = StartTransform.position;
            var directionToEmitter = (hitPosition - startPosition).normalized;
            hitPosition += directionToEmitter * ImpactOffset;

            _impactPs.transform.position = hitPosition;
            _impactPs.transform.LookAt(startPosition);

            LaunchLight.intensity = LightIntensity * _appearProgress;

            UpdateLineRenderer();
        }

        private void UpdateLineRenderer()
        {
            _lineRenderer.widthMultiplier = _appearProgress * LineRendererWidth;
            _lineRenderer.SetPosition(0, StartTransform.position);
            _lineRenderer.SetPosition(1, EndPosition);
        }
    }
}
