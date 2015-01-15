using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Threading;

namespace com.graphicated.unityeditorutils
{
		[UnityEditor.InitializeOnLoad]
		public class Dispatch
		{
				protected static volatile List<Action> _syncQueue = new List<Action> ();
			
				protected class Task
				{
				
						protected Action action;
				
						public Task (Action action)
						{
								this.action = action;
						}
				
						public void StartThread ()
						{
								ThreadPool.QueueUserWorkItem (Worker, null);
						}
				
						public void Worker (object state)
						{
								this.action.Invoke ();
						}
				}
	
				static Dispatch ()
				{
						EditorApplication.update += DispatchSync;
				}
			
				public void OnDestroy ()
				{
						EditorApplication.update -= DispatchSync;
				}
			
				public static void Async (Action action)
				{
						new Task (action).StartThread ();
				}
			
				public static void Sync (Action action)
				{
						lock (_syncQueue) {
								_syncQueue.Add (action);
						}
				}
		
				static void DispatchSync ()
				{	
						if (_syncQueue.Count > 0) {
								lock (_syncQueue) {
										foreach (Action action in _syncQueue) {
												action.Invoke ();
										}
										_syncQueue.Clear ();
								}
						}
			
				}
		}
}
