using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float visionRange = 15f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float hearingRange = 10f;
    [SerializeField] private float detectionTime = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float searchSpeed = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private float currentDetection = 0f;
    private EnemyState currentState = EnemyState.Patrol;
    private Vector3 lastKnownPosition;
    private float searchTimer = 0f;

    private enum EnemyState
    {
        Patrol,
        Chase,
        Search,
        Attack
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Search:
                UpdateSearch();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
        }

        UpdateDetection();
    }

    private void UpdatePatrol()
    {
        // 기본적인 순찰 로직
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = Random.insideUnitSphere * 10f;
            randomPoint += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void UpdateChase()
    {
        agent.SetDestination(player.position);
        agent.speed = chaseSpeed;
    }

    private void UpdateSearch()
    {
        searchTimer += Time.deltaTime;
        if (searchTimer >= 10f)
        {
            currentState = EnemyState.Patrol;
            searchTimer = 0f;
        }

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 searchPoint = lastKnownPosition + Random.insideUnitSphere * 5f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPoint, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    private void UpdateAttack()
    {
        // 공격 로직
        if (Vector3.Distance(transform.position, player.position) > 2f)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // 근접 공격 실행
            AttackPlayer();
        }
    }

    private void UpdateDetection()
    {
        bool canSeePlayer = CanSeePlayer();
        bool canHearPlayer = Vector3.Distance(transform.position, player.position) < hearingRange;

        if (canSeePlayer || canHearPlayer)
        {
            currentDetection += Time.deltaTime;
            if (currentDetection >= detectionTime)
            {
                currentState = EnemyState.Chase;
                lastKnownPosition = player.position;
            }
        }
        else
        {
            currentDetection = Mathf.Max(0, currentDetection - Time.deltaTime);
            if (currentDetection <= 0 && currentState == EnemyState.Chase)
            {
                currentState = EnemyState.Search;
                searchTimer = 0f;
            }
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < visionAngle / 2f && directionToPlayer.magnitude < visionRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRange))
            {
                return hit.transform.CompareTag("Player");
            }
        }
        return false;
    }

    public void DetectNoise(Vector3 noisePosition)
    {
        if (Vector3.Distance(transform.position, noisePosition) <= hearingRange)
        {
            currentDetection += 0.5f;
            lastKnownPosition = noisePosition;
            if (currentState == EnemyState.Patrol)
            {
                currentState = EnemyState.Search;
            }
        }
    }

    private void AttackPlayer()
    {
        // 공격 애니메이션 및 데미지 처리
        animator.SetTrigger("Attack");
        // 플레이어에게 데미지 주기
    }
}
