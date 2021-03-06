//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
namespace AnimationCommandsNameSpace
{
		public enum eAnimationCommands
		{
			Play = 0,
			PlayFromFrame,
			PlayFrom,
			Stop,
			Pause,
			Resume,
			SetFrame
		}

	public struct sAnimationCommand : IEquatable<sAnimationCommand>
	{
		public tk2dSpriteAnimator m_pAnimator;
		public int m_iAnimatorIndex;
		public eAnimationCommands m_eCommand;
		public string m_sClipname;
		public int m_iFrame;
		public float m_fTime;

        public override bool Equals(object obj)=>
            obj is sAnimationCommand command && Equals(command);


        public bool Equals(sAnimationCommand other) =>
            m_pAnimator == other.m_pAnimator
            && m_iAnimatorIndex == other.m_iAnimatorIndex
            && m_eCommand == other.m_eCommand
            && m_sClipname == other.m_sClipname
            && m_iFrame == other.m_iFrame
            && m_fTime == other.m_fTime;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_pAnimator) ? m_pAnimator.GetHashCode() : 0);
                hash = (hash * 7) + m_iAnimatorIndex.GetHashCode();
                hash = (hash * 7) + ((int)m_eCommand).GetHashCode();
                hash = (hash * 7) + (!Object.ReferenceEquals(null, m_sClipname) ? m_sClipname.GetHashCode() : 0);
                hash = (hash * 7) + m_iFrame.GetHashCode();
                hash = (hash * 7) + m_fTime.GetHashCode();
                return hash;
            }
        }
    }


}

