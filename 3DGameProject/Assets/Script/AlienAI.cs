using UnityEngine;
using UnityEngine.AI;

public class AlienAI : MonoBehaviour
{
    // 필수 컴포넌트 및 참조
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    private Animator animator; // 애니메이터 컴포넌트 참조

    // 몬스터 스탯
    public float health = 100;
    public int attackDamage = 10; // 플레이어에게 입힐 데미지

    // 순찰(Patrolling) 상태
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;

    // 공격(Attacking) 상태
    public float timeBetweenAttacks;
    private bool alreadyAttacked;

    // AI 상태 감지
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float hitStopTime = 0.5f; // 피격 애니메이션 길이(초) - Inspector에서 조절
    private void Awake()
    {
        player = GameObject.Find("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Animator 컴포넌트 할당
    }

    private void Update()
    {
        // 몬스터가 죽으면 아무것도 하지 않음
        if (health <= 0) return;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();

        // 현재 NavMeshAgent의 속도를 Animator의 Speed 파라미터에 전달
        // agent.velocity.magnitude는 현재 이동 속도를 나타냅니다.
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    // 플레이어의 공격을 받았을 때 호출될 함수
    public void TakeDamage(int damage)
    {
  

        health -= damage;
        animator.SetTrigger("GetHit");

        // 이동 정지
        StartCoroutine(HitStopCoroutine());

        if (health <= 0)
        {
            Debug.Log("몬스터가 사망했습니다");
            Die();
        }
    }

    private System.Collections.IEnumerator HitStopCoroutine()
    {
        if (agent != null)
            agent.isStopped = true;

        yield return new WaitForSeconds(hitStopTime);

        if (agent != null && health > 0) // 죽지 않았다면 다시 이동
            agent.isStopped = false;
    }

    private void Die()
    {
        // 1. Die 애니메이션 트리거
        animator.SetTrigger("Die");

        // 2. AI/이동/충돌 비활성화
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        // 3. (필요하다면) 스크립트 비활성화는 Destroy 직전에 하거나, 아예 하지 않아도 됨
        

        // 4. 5초 뒤에 오브젝트 제거
        Destroy(gameObject, 5f);
    }

    private void Patrolling()
    {
        agent.speed = 1.5f; // 걷는 속도 설정
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.speed = 4f; // 달리는 속도 설정
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position); // 공격 시 제자리 정지
        transform.LookAt(player); // 플레이어를 바라봄

        if (!alreadyAttacked)
        {
            // Attack 애니메이션 트리거 발동
            animator.SetTrigger("Attack");

            // 여기에 바로 데미지를 입히는 코드를 넣지 않습니다. (아래 3번 항목 참고)
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    // 공격 쿨다운 초기화
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // Animation Event로 호출될 함수 (매우 중요!)
    public void DealDamageToPlayer()
    {
        // 공격 애니메이션이 실행되는 도중, 플레이어가 여전히 공격 범위 안에 있는지 다시 한번 확인
        if (Physics.CheckSphere(transform.position, attackRange, whatIsPlayer))
        {
            Debug.Log("몬스터가 플레이어를 공격했습니다!");
            // 플레이어에게 데미지를 입히는 함수 호출 (플레이어에 PlayerHealth 같은 스크립트가 있다고 가정)
            player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet"))
        {
            TakeDamage(100); // 총알에 맞으면 100 데미지
            Destroy(other.gameObject); // 총알 파괴
        }
    }
    // 디버깅용 Gizmo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
