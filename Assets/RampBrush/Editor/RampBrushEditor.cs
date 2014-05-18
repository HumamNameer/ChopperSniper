/*
---------------------- Unity Ramp Brush ----------------------
--
-- Ramp Brush for Unity (Version 1.1)
-- Code by Ian Deane, based on Terrain Toolkit code by Sándor Moldán.
-- 

Copyright (c) 2010 Ian Deane

Developer hereby grants to Licensee a perpetual, non-exclusive, limited license 
to use the Software as set forth in this Agreement.
Licensee shall not modify, copy, duplicate, reproduce, license or sublicense
the Software, or transfer or convey the Software or any right in the Software
to anyone else without the prior written consent of Developer; provided that 
Licensee may make copies of the Software for backup or archival purposes.:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using UnityEditor;

// -------------------------------------------------------------------------------------------------------- EDITOR

[CustomEditor(typeof(RampBrush))]
public class RampBrushEditor : Editor {

    private bool keydown = false;
    
	private SerializedObject rampBrushObj;
	private SerializedProperty  brushSize, brushOpacity, brushSoftness, brushSampleDensity, brushSpacingJitter, controlPoints;
	private GUIContent controlPointsGUIC = new GUIContent("Control Points:");
	private GUIContent multiPointGUIC = new GUIContent("Do Curved Ramps");
	
	void OnEnable () {
		rampBrushObj = new SerializedObject(target);
		brushSize = rampBrushObj.FindProperty("brushSize");
		brushOpacity = rampBrushObj.FindProperty("brushOpacity");
		brushSoftness = rampBrushObj.FindProperty("brushSoftness");
		brushSampleDensity = rampBrushObj.FindProperty("brushSampleDensity");
		brushSpacingJitter = rampBrushObj.FindProperty("spacingJitter");
		controlPoints = rampBrushObj.FindProperty("controlPoints");
	}
	
	public override void OnInspectorGUI() {
		EditorGUIUtility.LookLikeControls();
		if (Application.isPlaying) {
			//return;
		}
		RampBrush rampBrush = (RampBrush) target as RampBrush;
		if (!rampBrush && !rampBrush.gameObject) {
			return;
		}
		rampBrushObj.Update();
		
		Terrain terComponent = (Terrain) rampBrush.GetComponent(typeof(Terrain));
		
		if (GUILayout.Button( "Activate/Inactivate Brush")) {
			rampBrush.toggleBrushOn();
		}

		EditorGUILayout.BeginHorizontal();
		
		string activeStr;
		if ( rampBrush.turnBrushOnVar ){
			activeStr = "\n\nbrush: active";
		} else {
			activeStr = "\n\nbrush: inactive";	
		}
		if ( rampBrush.multiPoint){
			activeStr += " curved ramps";
		} else {
			activeStr += " straight ramps";	
		}
		
		GUILayout.Label(activeStr);	
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		if (rampBrush.multiPoint){
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("    HINTS CURVED RAMPS:");
			EditorGUILayout.EndHorizontal();			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        1. Hold down the [CTRL][ALT] to add points");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        2. It is necessary to wiggle the mouse slightly to get the points to take");		
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        3. Click 'Stroke Path' to make the ramp");		
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        3. Click 'Clear Path' to clear the path and make another ramp");		
			EditorGUILayout.EndHorizontal();			
		} else {
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("    HINTS STRAIGHT RAMPS:");
			EditorGUILayout.EndHorizontal();			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        1. Hold down the [CTRL][ALT] to set the begin point (brush will turn red)");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        2. Hold down the [CTRL][SHIFT] key to set the end point. Can be repeated");		
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("        3. It is necessary to wiggle the mouse slightly to get the points to take");		
			EditorGUILayout.EndHorizontal();		
		}
		EditorGUILayout.Separator();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Brush size");
		rampBrush.brushSize = EditorGUILayout.Slider(brushSize.floatValue, 1, 600);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Opacity");
		rampBrush.brushOpacity = EditorGUILayout.Slider(brushOpacity.floatValue, 0, 1);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Softness");
		rampBrush.brushSoftness = EditorGUILayout.Slider(brushSoftness.floatValue, 0, 1);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Sample spacing (terrain units)");
		rampBrush.brushSampleDensity = EditorGUILayout.Slider(brushSampleDensity.floatValue, 1, 20);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Spacing Jitter");
		rampBrush.spacingJitter = EditorGUILayout.Slider(brushSpacingJitter.floatValue, 0f, 1f);
		EditorGUILayout.EndHorizontal();			
		rampBrush.multiPoint = EditorGUILayout.Toggle(multiPointGUIC,rampBrush.multiPoint);
		if (rampBrush.multiPoint){
			EditorGUILayout.PropertyField(controlPoints, controlPointsGUIC, true);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Stroke Path")){
	            Terrain ter = (Terrain) rampBrush.GetComponent(typeof(Terrain));
	            if (ter == null) {
	                return;
	            }			
	            TerrainData terData = ter.terrainData;
	            Undo.RegisterUndo(terData, "Ramp Brush");			
				rampBrush.StrokePath();
			}
			if (GUILayout.Button("Clear Path")){
				rampBrush.controlPoints.Clear();
			}
			EditorGUILayout.EndHorizontal();
		} else {
			rampBrush.beginRamp = EditorGUILayout.Vector3Field("begin ramp",rampBrush.beginRamp);
			rampBrush.endRamp = EditorGUILayout.Vector3Field("end ramp",rampBrush.endRamp);				
		}

		if (terComponent == null) {
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("The GameObject that Ramp Brush is attached to\n" +
									"does not have a Terrain component.\n" +
									"Please attach a Terrain component.",MessageType.Error);
			return;
		}
		rampBrushObj.ApplyModifiedProperties();
	}
	
    private void keyDown1(Vector3 hitPoint){
        RampBrush rampBrush = (RampBrush) target as RampBrush;
        if (Event.current.shift && Event.current.control) {
			if (rampBrush.multiPoint) return;
			rampBrush.endRamp = hitPoint;
			rampBrush.controlPoints.Clear();
			rampBrush.controlPoints.Add(rampBrush.beginRamp);
			rampBrush.controlPoints.Add(rampBrush.endRamp);
            Terrain ter = (Terrain) rampBrush.GetComponent(typeof(Terrain));
            if (ter == null) {
                return;
            }
            TerrainData terData = ter.terrainData;
            Undo.RegisterUndo(terData, "Ramp Brush");
			rampBrush.rampBrush();
			//rampBrush.StrokePath();
            rampBrush.beginRamp = rampBrush.endRamp;
        }
		if (Event.current.control && Event.current.alt) {
			if (rampBrush.multiPoint){
				rampBrush.controlPoints.Add(hitPoint);	
			} else {			
				rampBrush.beginRamp = hitPoint;
			}
		}
    }
    
    private void keyUp(){
 
    }
    
	public void OnSceneGUI() {
		RampBrush rampBrush = (RampBrush) target as RampBrush;
		if (rampBrush.turnBrushOnVar){

			rampBrush.isBrushHidden = false;
			// The editor does not get key events only mouse events.
			// Most mouse events "click", "other buttons", "shift click" etc... are spoken for
			// Simulate a key event by looking at modifyer keys when receive a mouse event
			if (Event.current.isMouse){
				Vector2 mouse = Event.current.mousePosition;
				mouse.y = Camera.current.pixelHeight - mouse.y + 20;
				Ray ray = Camera.current.ScreenPointToRay(mouse);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
					if (hit.transform.GetComponent("RampBrush")) {
						rampBrush.brushPosition = hit.point;						
					}
				}				
				
				if (Event.current.control && (Event.current.shift || Event.current.alt)){
					if (this.keydown == false){
						this.keyDown1(rampBrush.brushPosition);
					}
					this.keydown = true;		
				} else {
					if (this.keydown == true){
						this.keyUp();
					}
					this.keydown = false;
				}

                rampBrush.isBrushHidden = false;
				rampBrush.endRamp = hit.point;
//				if (Event.current.control && Event.current.alt){
					
//						rampBrush.beginRamp = hit.point;
//					}
//				} else {
//					
//				}
            }
		}
	}
}
