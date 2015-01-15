using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorIcons
{
		public struct AboutWindow
		{
				public static Texture2D MainHeader ()
				{
						return EditorGUIUtility.Load ("icons/aboutwindow.mainheader.png") as Texture2D;
				}
		}
		
		public static Texture2D AgeiaLogo ()
		{
				return EditorGUIUtility.Load ("icons/ageialogo.png") as Texture2D;
		}
		
		public struct Animation
		{
				public static Texture2D AddEvent ()
				{
						return EditorGUIUtility.Load ("icons/Animation.AddEvent.png") as Texture2D;
				}
				
				public static Texture2D AddKeyframe ()
				{
						return EditorGUIUtility.Load ("icons/Animation.AddKeyframe.png") as Texture2D;
				}
		}

		public static Texture2D Help ()
		{
				return EditorGUIUtility.Load ("icons/_Help.png") as Texture2D;
		}
	
		public static Texture2D Popup ()
		{
				return EditorGUIUtility.Load ("icons/_Popup.png") as Texture2D;
		}
		
}
