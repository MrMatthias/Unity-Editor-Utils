using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using System.Text;
using com.graphicated.unityeditorutils;

public class PNGExtract : EditorWindow
{

		[MenuItem("Window/PNG Extract")]
		public static void Init ()
		{
				PNGExtract window = (PNGExtract)EditorWindow.GetWindow (typeof(PNGExtract));
		}
		
		public class EditorPNG
		{
				public string path;
				public bool darkVersion;
				
				public string getPath (bool dark)
				{
						if (!dark) {
								return path;
						}
						string[] comps = path.Split ('/');
						comps [comps.Length - 1] = "d_" + comps [comps.Length - 1];
						string darkPath = "";
						foreach (var comp in comps) {
								darkPath += comp + "/";
						}
						return darkPath.Remove (darkPath.Length - 1);
				}
				
				public EditorPNG (string path, bool darkVersion)
				{
						this.path = path;
						this.darkVersion = darkVersion;
				}
				
		}
		
		public string res = "";
		public string[] pngs;
		protected Vector2 scrollpos = Vector2.zero;
		
		public void writeFoldoutVars (StreamWriter wr, Dictionary<string, object> tree, string path)
		{
				foreach (var key in tree.Keys) {
						if (tree [key].GetType () == typeof(Dictionary<string, object>)) {
								string foldoutvarname = path + key + "Foldout";
								foldoutvarname = foldoutvarname.Replace (" ", "_").Replace ("-", "_");
								wr.WriteLine ("\tprotected bool " + foldoutvarname + " = false;");
								writeFoldoutVars (wr, (Dictionary<string, object>)tree [key], path + key);
						}
				}
		}
		
		public void writeTextureVars (StreamWriter wr, Dictionary<string, object> tree, string path)
		{
				foreach (var key in tree.Keys) {
						if (tree [key].GetType () == typeof(Dictionary<string, object>)) {
								writeTextureVars (wr, (Dictionary<string, object>)tree [key], path + key);
						} else {
								EditorPNG p = tree [key] as EditorPNG;
								string varname = "d_" + path + key + "Texture";
								varname = varname.Replace (" ", "_").Replace ("-", "_");
								if (p.darkVersion) {
										wr.WriteLine ("\tprotected Texture2D " + varname + " = null;");
								}
								varname = path + key + "Texture";
								varname = varname.Replace (" ", "_").Replace ("-", "_");
								wr.WriteLine ("\tprotected Texture2D " + varname + " = null;");
						}
				}
		}
		
		public string Tabs (int t)
		{
				string res = "";
				for (int i=0; i<t; i++) {
						res += "\t";
				}
				return res;
		}
		
		public void writePNGField (StreamWriter wr, string path, string textureName, int tabs, EditorPNG p, bool dark)
		{
				wr.WriteLine (Tabs (tabs) + "if(" + textureName + " == null)");
				wr.WriteLine (Tabs (tabs) + "{");
				wr.WriteLine (Tabs (tabs + 1) + textureName + " = EditorGUIUtility.Load (\"" + p.getPath (dark) + "\") as Texture2D;");
				wr.WriteLine (Tabs (tabs) + "}");
		
				wr.WriteLine (Tabs (tabs) + "EditorGUILayout.BeginHorizontal ();");
				wr.WriteLine (Tabs (tabs) + "guitContent.image = " + textureName + ";");
				wr.WriteLine (Tabs (tabs) + "EditorGUILayout.TextField(\"" + p.getPath (dark) + "\");");
				wr.WriteLine (Tabs (tabs) + "GUILayout.Label(guitContent);");
				wr.WriteLine (Tabs (tabs) + "EditorGUILayout.EndHorizontal ();");
				
		}
		
		public void writePNGViews (StreamWriter wr, Dictionary<string, object> tree, string path, int tabs, string filepath)
		{
				foreach (var key in tree.Keys) {
						if (tree [key].GetType () == typeof(Dictionary<string, object>)) {
								string foldoutVarname = path + key + "Foldout";
								foldoutVarname = foldoutVarname.Replace (" ", "_").Replace ("-", "_");
								
								wr.WriteLine (Tabs (tabs) + "EditorGUI.indentLevel += 1;");
								wr.WriteLine (Tabs (tabs) + "EditorGUILayout.BeginHorizontal ();");
								wr.WriteLine (Tabs (tabs) + "EditorGUILayout.BeginVertical ();");
								wr.WriteLine (Tabs (tabs) + foldoutVarname + " = EditorGUILayout.Foldout(" + foldoutVarname + ", \"" + path + key + "\");");
								wr.WriteLine (Tabs (tabs) + "if(" + foldoutVarname + ")");
								wr.WriteLine (Tabs (tabs) + "{");
								writePNGViews (wr, (Dictionary<string, object>)tree [key], path + key, tabs + 1, path + "/" + key);
								wr.WriteLine (Tabs (tabs) + "}");
								wr.WriteLine (Tabs (tabs) + "EditorGUILayout.EndVertical ();");
								wr.WriteLine (Tabs (tabs) + "EditorGUILayout.EndHorizontal ();");
								wr.WriteLine (Tabs (tabs) + "EditorGUI.indentLevel -= 1;");
								
								
								
						} else {
								EditorPNG p = tree [key] as EditorPNG;
								
								string textureName = path + key + "Texture";
								textureName = textureName.Replace (" ", "_").Replace ("-", "_");
								writePNGField (wr, filepath + key.Substring (0, key.Length - 4) + ".png", textureName, tabs, p, false);
								if (p.darkVersion) {
										textureName = "d_" + path + key + "Texture";
										textureName = textureName.Replace (" ", "_").Replace ("-", "_");
										writePNGField (wr, filepath + "d_" + key, textureName, tabs, p, true);
								}
						}
				}
		}
		
		public void createEditorClass (Dictionary<string, object> tree, string filepath)
		{
				FileStream fs = null;
				if (! File.Exists (filepath)) {
						fs = File.Create (filepath);
				} else {
						fs = File.OpenWrite (filepath);
				}
				StreamWriter wr = new StreamWriter (fs);
			
				wr.WriteLine ("using UnityEngine;");
				wr.WriteLine ("using UnityEditor;");
				wr.WriteLine ("using System.Collections;");
				wr.WriteLine ("using System.Collections.Generic;");
				wr.WriteLine ("");
				wr.WriteLine ("public class EditorImagesViewerWindow : EditorWindow");
				wr.WriteLine ("{");
		
				wr.WriteLine ("\t[MenuItem(\"Window/Editor Images Viewer\")]");
				wr.WriteLine ("\tpublic static void Init()");
				wr.WriteLine ("\t{");
				wr.WriteLine ("\t\tEditorImagesViewerWindow window = (EditorImagesViewerWindow) EditorWindow.GetWindow(typeof(EditorImagesViewerWindow));");
				wr.WriteLine ("\t}");
		
				wr.WriteLine ("");
				writeFoldoutVars (wr, tree, "");
				wr.WriteLine ("");
				
				writeTextureVars (wr, tree, "");
				wr.WriteLine ("private GUIContent guitContent = new GUIContent ();");
				wr.WriteLine ("");
				
				wr.WriteLine ("Vector2 scrollpos = Vector2.zero;");
				wr.WriteLine ("");
				
				
				wr.WriteLine ("\tpublic void OnGUI()");
				wr.WriteLine ("\t{");
				wr.WriteLine ("\t\tscrollpos = GUILayout.BeginScrollView (scrollpos);");
				
				writePNGViews (wr, tree, "", 2, "");
				
				wr.WriteLine ("\t\tGUILayout.EndScrollView ();");
				wr.WriteLine ("\t}");
		
				wr.WriteLine ("}");

				wr.Close ();
				fs.Close ();
				Dispatch.Sync (() => {
						UnityEngine.Debug.Log ("Done writing class");
				});
		}
		
		
		public Dictionary<string, object> generateTree (List<string> paths)
		{
				Dictionary<string, object> tree = new Dictionary<string, object> ();
				foreach (string path in paths) {
						if (string.IsNullOrEmpty (path)) {
								continue;
						}
						Texture2D texture = EditorGUIUtility.Load (path) as Texture2D;
						if (texture == null) {
								continue;
						}
						
						UnityEngine.Debug.Log ("adding: " + path);
						
			
						string[] components = path.Split ('/');
						Dictionary<string, object> subDict = tree;
						int i = 0;
						while (i < components.Length - 1) {
								var comp = components [i];
								
								if (! subDict.ContainsKey (comp)) {
										subDict [comp] = new Dictionary<string, object> ();
										subDict = subDict [comp] as Dictionary<string, object>;
								} else {
										if (subDict [comp].GetType () == typeof(Dictionary<string, object>)) {
												subDict = (Dictionary<string, object>)subDict [comp];
										} else {
												Dispatch.Sync (() => {
														UnityEngine.Debug.LogError ("Unexpected type: " + subDict [comp].GetType ().ToString ());
												});
										}
								}
								i++;
						}
			
						string[] nameComponents = components [i].Split ('.');
						
						int j = 0;
						while (j<nameComponents.Length -2) {
								var comp = nameComponents [j];
								
								if (! subDict.ContainsKey (comp)) {
										subDict [comp] = new Dictionary<string, object> ();
										subDict = (Dictionary<string, object>)subDict [comp];
								} else {
										if (subDict [comp].GetType () == typeof(Dictionary<string, object>)) {
												subDict = (Dictionary<string, object>)subDict [comp];
										} else {
												Dispatch.Sync (() => {
														UnityEngine.Debug.LogError ("unexpected type 2: " + subDict [comp].GetType ().Name);
												});
										}
								}
								j++;
						}
						
						string filename = nameComponents [j] + "_png";
						bool darkVersion = false;
						darkVersion = filename.StartsWith ("d_");
						if (darkVersion) {
								filename = filename.Substring (2);
						}
			
						object entry; 
						if (! subDict.TryGetValue (filename, out entry)) {
								subDict [filename] = new EditorPNG (path, darkVersion);
						} else {
								EditorPNG e = (EditorPNG)entry;
								if (! e.darkVersion) {
										e.darkVersion = darkVersion;
								}
						}
				}
				Dispatch.Sync (() => {
						
			
						UnityEngine.Debug.Log (paths.Count);
				});
				return tree;
		}
		
		public void OnGUI ()
		{
				if (GUILayout.Button ("Extract")) {
						string dataPath = Application.dataPath;
//						Dispatch.Async (() => {
						Process p = new Process ();
						p.StartInfo.FileName = "egrep";
						string filepath = (EditorApplication.applicationContentsPath + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "unity editor resources");
						UnityEngine.Debug.Log ("filepath: " + filepath);
						p.StartInfo.Arguments = "-a -o \"[a-zA-Z0-9 ._ -/]*\\.png\" \"" + filepath + "\"";
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.RedirectStandardOutput = true;
						p.StartInfo.RedirectStandardError = true;
						StringBuilder builder = new StringBuilder ();
						List<string> paths = new List<string> ();
						p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
								paths.Add (e.Data);
								builder.Append (e.Data + Environment.NewLine);
						};
								
						p.Start ();
						p.BeginOutputReadLine ();
			
								
						p.WaitForExit ();
						if (p.HasExited) {
								Dispatch.Sync (() => {
										UnityEngine.Debug.Log ("Creating tree");
								});
								Dictionary<string, object> tree = generateTree (paths);
								Dispatch.Sync (() => {
										UnityEngine.Debug.Log ("Creating class");
								});
								createEditorClass (tree, dataPath + "/EditorImagesViewerWindow.cs");
								Dispatch.Sync (() => {
										UnityEngine.Debug.Log ("Java File at: " + dataPath + "/EditorImagesViewerWindow.cs");
										res = builder.ToString ();	
								});
										
						} else {
								Dispatch.Sync (() => {
										UnityEngine.Debug.Log ("not exited");
								});
						}
//						});
				}
				scrollpos = GUILayout.BeginScrollView (scrollpos);
				GUILayout.TextArea (res, GUILayout.ExpandHeight (true));
				GUILayout.EndScrollView ();
				if (GUILayout.Button ("check size")) {
						UnityEngine.Debug.Log ("items: " + pngs.Length);
				}
				
		}
}
