using UnityEngine;

public class Movement3D : MonoBehaviour
{
	[SerializeField]
	private	float		moveSpeed = 5.0f;	
	[SerializeField]
	private	float		gravity = -9.81f;	
	[SerializeField]
	private	float		jumpForce = 3.0f;	
	private	Vector3		moveDirection;
    [SerializeField]
    private float rotationSpeed = 360.0f;
    [SerializeField]
	private	Transform			cameraTransform;
	private	CharacterController	characterController;

	public float MoveSpeed
	{ 
		get { return moveSpeed; }
	}
	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
	}

	private void Update()
	{
		
		if ( characterController.isGrounded == false )
		{
			moveDirection.y += gravity * Time.deltaTime;
		}

		characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        
    }

	public void MoveTo(Vector3 direction)
	{
		
		Vector3 movedis = cameraTransform.rotation * direction;
		moveDirection	= new Vector3(movedis.x, moveDirection.y, movedis.z);
	}

	public void JumpTo()
	{
		if ( characterController.isGrounded == true )
		{
			moveDirection.y = jumpForce;
		}
	}
}

