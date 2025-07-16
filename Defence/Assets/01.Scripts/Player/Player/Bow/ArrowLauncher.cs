//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit.Interactables;

//public class ArrowLauncher : MonoBehaviour
//{
//    [Header("Launch Settings")]
//    [SerializeField] private float _speed = 10f;

//    [Header("Visual Effects")]
//    [SerializeField] private GameObject _trailSystem;

//    private Rigidbody _rigidBody;
//    private bool _inAir = false;
//    private XRPullInteractable _pullInteractable;

//    private void Awake()
//    {
//        InitializeComponents();
//        SetPhysics(false);
//    }

//    private void InitializeComponents()
//    {
//        _rigidBody = GetComponent<Rigidbody>();
//        if(_rigidBody == null)
//        {
//            Debug.LogError($"Rigidbody component not found on Arrow {gameObject.name}");
//        }
//    }

//    public void Initialize(XRPullInteractable pullInteractable)
//    {
//        _pullInteractable = pullInteractable;
//        _pullInteractable.PullActionReleased += Release;
//    }
//    private void OnDestroy()
//    {
//        if(_pullInteractable != null)
//        {
//            _pullInteractable.PullActionReleased -= Release;
//        }
//    }

//    private void Release(float value)
//    {
//        if (_pullInteractable != null)
//        {
//            _pullInteractable.PullActionReleased -= Release;
//        }

//        gameObject.transform.parent = null;
//        _inAir = true;
//        SetPhysics(true);

//        Vector3 force = transform.forward * value * _speed;
//        _rigidBody.AddForce(force, ForceMode.Impulse);

//        StartCoroutine(RotateWithVelocity());

//        _trailSystem.SetActive(true);
//    }

//    private IEnumerator RotateWithVelocity()
//    {
//        yield return new WaitForFixedUpdate();
//        while (_inAir)
//        {
//            if(_rigidBody != null && _rigidBody.LinearVelocity.sqrMagnitude > 0.01f)
//            {
//                transform.rotation = Quaternion.LookRotation(_rigidBody.linearVelocity, transform.up);
//            }
//            yield return null;
//        }
//    }

//    public void StopFlight()
//    {
//        _inAir = false;
//        SetPhysics(false);
//        _trailSystem.SetActive(false);
//    }
//}