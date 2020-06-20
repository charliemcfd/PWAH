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

		//Truncate the calcualted position to an integer and store in a seperate variable
		int resolutionPositionInt = Mathf.FloorToInt(resolutionPosition);

		//Subtract the integer component of the resolution to get a decimal "remainder"
		resolutionPosition -= resolutionPositionInt;

		//If the reaminder is greater than or equal to 0.5 (half a pixel) then increase the value of the integer position
		if (resolutionPosition >= 0.5)
		{
			resolutionPositionInt++;
		}

		//Convert the resolution position back into Unity co-ordinate space by multiplying by the reciprocal (1/PixelsPerUnit)
		return resolutionPositionInt * m_pixelsPerUnitReciprocal;

	}
}
