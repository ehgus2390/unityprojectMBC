//using UnityEngine;

//public class Rifle : Weapon
//{
//    [SerializeField] private float range = 100f;
//    [SerializeField] private LayerMask targetLayers;
//    [SerializeField] private float recoilAmount = 0.1f;
//    [SerializeField] private float recoilRecoverySpeed = 2f;

//    private Vector3 originalPosition;
//    private Vector3 currentRecoil;

//    protected override void Start()
//    {
//        base.Start();
//        originalPosition = transform.localPosition;
//    }
//    public interface IDamageable
//    {
//        void TakeDamage(float damage);
//    }
//    protected override void PerformFire()
//    {
//        // 반동 효과
//        ApplyRecoil();

//        // 레이캐스트로 발사
//        RaycastHit hit;
//        if (Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out hit, range, targetLayers))
//        {
//            // 히트 이펙트 생성
//            if (hit.transform.TryGetComponent<IDamageable>(out var damageable))
//            {
//                damageable.TakeDamage(weaponData.damage);
//            }
//        }
//    }

//    private void ApplyRecoil()
//    {
//        currentRecoil += new Vector3(
//            Random.Range(-recoilAmount, recoilAmount),
//            Random.Range(0, recoilAmount),
//            Random.Range(-recoilAmount, recoilAmount)
//        );
//    }

//    private void Update()
//    {
//        // 반동 복구
//        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
//        transform.localPosition = originalPosition + currentRecoil;
//    }
//}
