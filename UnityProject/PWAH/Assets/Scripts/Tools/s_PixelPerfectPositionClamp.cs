using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_PixelPerfectPositionClamp : MonoBehaviour
{

	protected float m_pixelsPerUnit; //Per meter
	protected float m_pixelsPerUnitReciprocal;
	protected Rigidbody2D m_rigidBody;

	// Start is called before the first frame update
	void Start()
    {
		m_pixelsPerUnit = 100;
		m_pixelsPerUnitReciprocal = 1.0f / m_pixelsPerUnit;
		m_rigidBody = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void LateUpdate()
    {
		//If the entity has a rigidbody, only allow clamping when it is at rest.
		bool canClamp = true;

		if(m_rigidBody && m_rigidBody.velocity.normalized.sqrMagnitude != 0.0f)
		{
			canClamp = false;
		}

		if (canClamp)
		{
			float unperfectX = transform.position.x;
			float unperfectY = transform.position.y;

			float clampedX = CalculatePixelPerfectPosition(unperfectX);
			float clampedY = CalculatePixelPerfectPosition(unperfectY);

			transform.position = new Vector3(clampedX, clampedY, transform.position.z);
		}
		
	}

	protected float CalculatePixelPerfectPosition(float unperfectPosition)
	{

		//Calculate the position in pixel-space (Still  a floating point number at this point)
		float resolutionPosition = unperfectPosition * m_pixelsPerUnit;

		/*Round the position to an integer. This means that if the position's fractional portion is >= 0.5, the pixel value
		 * will be rounded up, otherwise it will be rounded down.
		*/
		int resolutionPositionInt = Mathf.RoundToInt(resolutionPosition);

		//Convert the resolution position back into Unity co-ordinate space by multiplying by the reciprocal (1/PixelsPerUnit)
		return resolutionPositionInt * m_pixelsPerUnitReciprocal;

	}
}
