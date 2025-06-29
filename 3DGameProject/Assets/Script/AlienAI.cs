using UnityEngine;
using UnityEngine.AI;

public class AlienAI : MonoBehaviour
{
    // �ʼ� ������Ʈ �� ����
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    private Animator animator; // �ִϸ����� ������Ʈ ����

    // ���� ����
    public float health = 100;
    public int attackDamage = 10; // �÷��̾�� ���� ������

    // ����(Patrolling) ����
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;

    // ����(Attacking) ����
    public float timeBetweenAttacks;
    private bool alreadyAttacked;

    // AI ���� ����
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float hitStopTime = 0.5f; // �ǰ� �ִϸ��̼� ����(��) - Inspector���� ����
    private void Awake()
    {
        player = GameObject.Find("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Animator ������Ʈ �Ҵ�
    }

    private void Update()
    {
        // ���Ͱ� ������ �ƹ��͵� ���� ����
        if (health <= 0) return;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInSightRange && playerInAttackRange) AttackPlayer();

        // ���� NavMeshAgent�� �ӵ��� Animator�� Speed �Ķ���Ϳ� ����
        // agent.velocity.magnitude�� ���� �̵� �ӵ��� ��Ÿ���ϴ�.
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    // �÷��̾��� ������ �޾��� �� ȣ��� �Լ�
    public void TakeDamage(int damage)
    {
  

        health -= damage;
        animator.SetTrigger("GetHit");

        // �̵� ����
        StartCoroutine(HitStopCoroutine());

        if (health <= 0)
        {
            Debug.Log("���Ͱ� ����߽��ϴ�");
            Die();
        }
    }

    private System.Collections.IEnumerator HitStopCoroutine()
    {
        if (agent != null)
            agent.isStopped = true;

        yield return new WaitForSeconds(hitStopTime);

        if (agent != null && health > 0) // ���� �ʾҴٸ� �ٽ� �̵�
            agent.isStopped = false;
    }

    private void Die()
    {
        // 1. Die �ִϸ��̼� Ʈ����
        animator.SetTrigger("Die");

        // 2. AI/�̵�/�浹 ��Ȱ��ȭ
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        // 3. (�ʿ��ϴٸ�) ��ũ��Ʈ ��Ȱ��ȭ�� Destroy ������ �ϰų�, �ƿ� ���� �ʾƵ� ��
        

        // 4. 5�� �ڿ� ������Ʈ ����
        Destroy(gameObject, 5f);
    }

    private void Patrolling()
    {
        agent.speed = 1.5f; // �ȴ� �ӵ� ����
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
        agent.speed = 4f; // �޸��� �ӵ� ����
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position); // ���� �� ���ڸ� ����
        transform.LookAt(player); // �÷��̾ �ٶ�

        if (!alreadyAttacked)
        {
            // Attack �ִϸ��̼� Ʈ���� �ߵ�
            animator.SetTrigger("Attack");

            // ���⿡ �ٷ� �������� ������ �ڵ带 ���� �ʽ��ϴ�. (�Ʒ� 3�� �׸� ����)
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    // ���� ��ٿ� �ʱ�ȭ
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // Animation Event�� ȣ��� �Լ� (�ſ� �߿�!)
    public void DealDamageToPlayer()
    {
        // ���� �ִϸ��̼��� ����Ǵ� ����, �÷��̾ ������ ���� ���� �ȿ� �ִ��� �ٽ� �ѹ� Ȯ��
        if (Physics.CheckSphere(transform.position, attackRange, whatIsPlayer))
        {
            Debug.Log("���Ͱ� �÷��̾ �����߽��ϴ�!");
            // �÷��̾�� �������� ������ �Լ� ȣ�� (�÷��̾ PlayerHealth ���� ��ũ��Ʈ�� �ִٰ� ����)
            player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet"))
        {
            TakeDamage(100); // �Ѿ˿� ������ 100 ������
            Destroy(other.gameObject); // �Ѿ� �ı�
        }
    }
    // ������ Gizmo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
