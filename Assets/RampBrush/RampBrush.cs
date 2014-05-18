//----------------------------------------------
//            RampBrush
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------

using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

[ExecuteInEditMode()]
[AddComponentMenu("Terrain/Ramp Brush")]

public class RampBrush : MonoBehaviour {	
	bool VERBOSE = false; 
	
	public bool brushOn = false;
	public bool turnBrushOnVar = false;
	
	public bool isBrushHidden = false;
	public Vector3 brushPosition;
	public Vector3 beginRamp;
	public Vector3 endRamp;
	public float brushSize = 50.0f;
	public float brushOpacity = 1.0f;
	public float brushSoftness = 0.5f;
	public float brushSampleDensity = 4.0f;
	public bool shiftProcessed= true;
    public Vector3 backupVector;
	public int numSubDivPerSeg = 10;
	
	public float spacingJitter;
	public float sizeJitter;
    
	public bool multiPoint;
	public List<Vector3> controlPoints = new List<Vector3>();
	List<float> _distBetweenPoints = new List<float>();
	float _totalLength;
	float _totalLengthPixels;
	
	public void OnDrawGizmos() {
		if (turnBrushOnVar){
			Terrain ter = (Terrain) GetComponent(typeof(Terrain));
			if (ter == null) {
				return;
			}

			// Brush gizmos...
//			if (isBrushPainting) {
//				Gizmos.color = Color.red;
//			} else {
				Gizmos.color = Color.cyan;
//			}
			float crossHairSize = brushSize / 4.0f;
			Gizmos.DrawLine((brushPosition + new Vector3(-crossHairSize, 0, 0)), (brushPosition + new Vector3(crossHairSize, 0, 0)));
			Gizmos.DrawLine(brushPosition + new Vector3(0, -crossHairSize, 0), brushPosition + new Vector3(0, crossHairSize, 0));
			Gizmos.DrawLine(brushPosition + new Vector3(0, 0, -crossHairSize), brushPosition + new Vector3(0, 0, crossHairSize));
			Gizmos.DrawWireCube(brushPosition, new Vector3(brushSize, 0, brushSize));
			Gizmos.DrawWireSphere(brushPosition, brushSize / 2);
			
			if (!multiPoint){
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(beginRamp,brushSize / 2f);
			} else {
				Gizmos.color = Color.magenta;
				for (int i = 0; i < controlPoints.Count; i++){
					Gizmos.DrawWireSphere(controlPoints[i],brushSize / 2f);
				}
				if (controlPoints.Count > 2){
					double tstep = 1.0 / ((controlPoints.Count-1.0) * 8) - 10e-15; //8 subsec per segment subtract tiny amount
					//StringBuilder sb = new StringBuilder();
					//sb.AppendLine("NumPoints = " + controlPoints.Count);
					calculateDistBetweenPoints(controlPoints);
					Ray ra = parameterizedLine(0f,controlPoints,null);
					double t = tstep;
					int n = 0; //todo remove if possible
					while (t <= 1.0f && n < 1000){
						Ray rb = parameterizedLine((float) t,controlPoints,null);
						Gizmos.DrawLine(ra.origin,rb.origin);	
						ra = rb;
						t += tstep;
						n++;
					}
				}
				//Debug.Log(sb);
			}
			
			//==========================
			
//				Gizmos.color = Color.blue;
//				Gizmos.DrawWireCube(beginClipPlane.distance * beginClipPlane.normal, Vector3.one * 10f);
//				Gizmos.DrawRay(new Ray(beginClipPlane.distance * beginClipPlane.normal, beginClipPlane.normal * 20));
//			
//				Gizmos.color = Color.blue;
//				Gizmos.DrawWireCube(endClipPlane.distance * beginClipPlane.normal, Vector3.one * 10f);
//				Gizmos.DrawRay(new Ray(endClipPlane.distance * beginClipPlane.normal, endClipPlane.normal * 20));
			
			//todo
//			Gizmos.color = Color.blue;
//			Gizmos.DrawRay(beginClipPlaneOrig, beginClipPlane.normal * 50f);
//			Gizmos.DrawRay(endClipPlaneOrig, endClipPlane.normal * 50f);
		}
	}
	
	
	public int[] terrainCordsToBitmap(TerrainData terData, Vector3 v){
			float Tw = (float) terData.heightmapWidth;
			float Th = (float) terData.heightmapHeight;
			Vector3 Ts = terData.size;
			int ny = (int) Mathf.Floor((Tw / Ts.x) * v.x);
		    int nx = (int) Mathf.Floor((Th / Ts.z) * v.z);
			int[] o;
			o = new int[2];
			o[0] = nx;
			o[1] = ny;
			return o;
	}
	
	public float[] bitmapCordsToTerrain(TerrainData terData, int x, int y){
			int Tw = terData.heightmapWidth;
			int Th = terData.heightmapHeight;
			Vector3 Ts = terData.size;
		
			float ny = (x)* (Ts.z / Th) ;
		    float nx = (y) *(Ts.x / Tw) ;
			float[] o;
			o = new float[2];
			o[0] = nx;
			o[1] = ny;
			return o;
	}	
	
	public void toggleBrushOn(){
		if (turnBrushOnVar){
			turnBrushOnVar = false;
		} else {
			turnBrushOnVar = true;
		}
	}
	
	public void rampBrush() {
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		if (ter == null) {
			Debug.LogError("No terrain component on this GameObject");
			return;
		}		
		int Px = 0;
		int Py = 0;		
		try { 
			TerrainData terData = ter.terrainData;
			int Tw = terData.heightmapWidth; //heightMapResolution
			int Th = terData.heightmapHeight;//heightMapResolution
			Vector3 Ts = terData.size; //x and z and height of terrain
			
			if (VERBOSE) Debug.Log("terrainData heightmapHeight/heightmapWidth:" + Tw + " " + Tw);
			if (VERBOSE) Debug.Log("terrainData heightMapResolution:" + terData.heightmapResolution);
			if (VERBOSE) Debug.Log("terrainData size:" + terData.size);
			
			Vector3 ls = transform.localScale; //need to remember and reset the scale because the InverseTransform does not work properly for terrains because they ignore the scale
			transform.localScale = new Vector3(1.0f,1.0f,1.0f);
						
			Vector3 localBeginPosition = transform.InverseTransformPoint(beginRamp);
			Vector3 localEndPosition = transform.InverseTransformPoint(endRamp);
			
			transform.localScale = ls;
			
//			int Sx = (int) Mathf.Floor((Tw / Ts.x) * brushSize);
//			int Sz = (int) Mathf.Floor((Th / Ts.z) * brushSize);

			int Sx = (int) Mathf.Floor((Tw / Ts.z) * brushSize); //the x and z are mixed up intentionally
			int Sz = (int) Mathf.Floor((Th / Ts.x) * brushSize); //the x and z are mixed up intentionally
			
			int[] pi = terrainCordsToBitmap(terData,localBeginPosition);
			int[] pf = terrainCordsToBitmap(terData,localEndPosition);
			
			if (pi[0] < 0 || pf[0] < 0 || pi[1] < 0 || pf[1] < 0 ||
				pi[0] >= Tw || pf[0] >= Tw || pi[1] >= Th || pf[1] >= Th){
				Debug.LogError("The start point or the end point was out of bounds. Make sure the gizmo is over the terrain before setting the start and end points." +
					           "Note: that sometimes Unity does not update the collider after changing settings in the 'Set Resolution' dialog. Entering play mode should reset the collider.");
				return;
			}
			
            double distBetween = Math.Sqrt((pf[0]-pi[0])*(pf[0]-pi[0]) + (pf[1]-pi[1])*(pf[1]-pi[1]));
            
			float[,] heightMapAll = terData.GetHeights(0, 0, Tw, Th);
			
			
			//calculate plane. n is plane normal, localEnd and localBegin are in plane.
			//plane uses terrain cord system, y coordinate is in terrain height units, not world units.
			localEndPosition.y = heightMapAll[pf[0],pf[1]];
			localBeginPosition.y = heightMapAll[pi[0],pi[1]];
			
			Vector3 v1 = localEndPosition - localBeginPosition;
			Vector3 v2 = new Vector3(-v1.z, 0.0f, v1.x);
			Vector3 n = Vector3.Cross(v1,v2);
			n.Normalize();		
			
			Vector3 vclip = new Vector3(v1.x, 0.0f, v1.z);

			//parameterized line
			float timeIncrement;
			if (brushSize < 15){
				timeIncrement = (brushSize/6)/v1.magnitude;
			} else {
				timeIncrement = (float) (1.0 / distBetween * brushSampleDensity);
			}

			if (VERBOSE){
				float[] temp = bitmapCordsToTerrain(terData,pi[0],pi[1]);
				Debug.Log("Local Begin Pos:" + localBeginPosition);
				Debug.Log("pixel begin coord:" + pi[0] + " " + pi[0]);
				Debug.Log("Local begin Pos Rev Transformed:" + temp[0] + " " + temp[1]);
				temp = bitmapCordsToTerrain(terData,pf[0],pf[1]);
				Debug.Log("Local End Pos:" + localEndPosition);
				Debug.Log("pixel End coord:" + pf[0] + " " + pf[1]);
				Debug.Log("Local End Pos Rev Transformed:" + temp[0] + " " + temp[1]);		
				Debug.Log("Sample Width/height: " + Sx + " " + Sz);
				Debug.Log("Brush Width: " + timeIncrement);
			}			
			
			for(float t = 0.0f; t <= 1.0f; t += timeIncrement){
				Vector3 psamp = localBeginPosition + t * (v1);
				
				int[] pv = terrainCordsToBitmap(terData, psamp);
				Px = pv[0] - Sx/2;
				Py = pv[1] - Sz/2; 
				 
				float[,] heightMap = new float[Sx,Sz]; //little samples size heightmap
				for (int i = 0; i < Sx; i ++){
					for (int j = 0; j <  Sz; j++){
						if (Px + i >= 0 && Py + j >=0 && Px + i < Tw && Py + j < Th) {
							heightMap[i,j] = heightMapAll[Px + i,Py + j] ;
						} else {
							heightMap[i,j] = 0;
						}
					}
				}				
				
				Sx = heightMap.GetLength(0);
				Sz = heightMap.GetLength(1);
				// build a perfect ramp 
				float[,] erodedheightMap = (float[,]) heightMap.Clone();
				for (int Tx = 0; Tx < Sx; Tx++) {
					for (int Tz = 0; Tz < Sz; Tz++) {
						float[] My = bitmapCordsToTerrain(terData, (Px + Tx), (Py + Tz));
						//don't ramp points before begin position or after end position
						Boolean clipThisPoint = false;
						if (vclip.x*(My[0] - localBeginPosition.x) + vclip.z*(My[1] - localBeginPosition.z) < 0){
							clipThisPoint = true;
						} else if (-vclip.x*(My[0] - localEndPosition.x) - vclip.z*(My[1] - localEndPosition.z) < 0){
							clipThisPoint = true;
						}	
						if (!clipThisPoint){
							erodedheightMap[Tx,Tz] =  localBeginPosition.y - (n.x*(My[0]-localBeginPosition.x) + n.z*(My[1] - localBeginPosition.z))/n.y;
						}
					}
				}
			
				// Apply it to the terrain object
				float sampleRadius = Sx / 2.0f;
				for (int Tx = 0; Tx < Sx; Tx++) {
					for (int Ty = 0; Ty < Sz; Ty++) {
						
						float newHeightAtPoint = erodedheightMap[Tx, Ty];
						float oldHeightAtPoint = heightMap[Tx, Ty];
						
						float distancefromOrigin = Vector2.Distance(new Vector2(Tx, Ty), new Vector2(sampleRadius, sampleRadius));
						float weightAtPoint = 1.0f - ((distancefromOrigin - (sampleRadius - (sampleRadius * brushSoftness))) / (sampleRadius * brushSoftness));
						if (weightAtPoint < 0.0f) {
							weightAtPoint = 0.0f;
						} else if (weightAtPoint > 1.0f) {
							weightAtPoint = 1.0f;
						}
						weightAtPoint *= brushOpacity;
						float blendedHeightAtPoint = (newHeightAtPoint * weightAtPoint) + (oldHeightAtPoint * (1.0f - weightAtPoint));
						heightMap[Tx, Ty] = blendedHeightAtPoint;
						
					}
				}
				
				for (int i = 0; i < Sx; i ++){
					for (int j = 0; j <  Sz; j++){
						if (Px + i >= 0 && Py + j >=0 && Px + i < Tw && Py + j < Th) {
							heightMapAll[Px + i,Py + j] = heightMap[i,j];
						}
					}
				}
			}
			terData.SetHeights(0,0,heightMapAll);
			//beginRamp = endRamp;
		} catch (Exception e) {
		 	Debug.LogError("A brush error occurred: "+e);
		}			
	} 
	
	public void StrokePath() {
		_StrokePath();
	}
	
	public void _StrokePath() {
		Terrain ter = (Terrain) GetComponent(typeof(Terrain));
		if (ter == null) {
			Debug.LogError("No terrain component on this GameObject");
			return;
		}		
		int Px = 0;
		int Py = 0;		
		try { 
			TerrainData terData = ter.terrainData;
			int Tw = terData.heightmapWidth; //heightMapResolution in pixels
			int Th = terData.heightmapHeight;//heightMapResolution in pixels
			Vector3 Ts = terData.size; //x and z and height of terrain
			
			if (VERBOSE) Debug.Log("terrainData heightmapHeight/heightmapWidth:" + Tw + " " + Tw);
			if (VERBOSE) Debug.Log("terrainData heightMapResolution:" + terData.heightmapResolution);
			if (VERBOSE) Debug.Log("terrainData size:" + terData.size);
			
			Vector3 ls = transform.localScale; //need to remember and reset the scale because the InverseTransform does not work properly for terrains because they ignore the scale
			transform.localScale = new Vector3(1.0f,1.0f,1.0f);
			List<Vector3> controlPointsLocal = new List<Vector3>();//[controlPoints.Count];
			for (int i = 0; i < controlPoints.Count; i++){
				controlPointsLocal.Add( transform.InverseTransformPoint(controlPoints[i]) );
			}
			
			//Vector3 localBeginPosition = transform.InverseTransformPoint(beginRamp);
			//Vector3 localEndPosition = transform.InverseTransformPoint(endRamp);
			
			transform.localScale = ls;
			
//			int[] pi = terrainCordsToBitmap(terData,localBeginPosition);
//			int[] pf = terrainCordsToBitmap(terData,localEndPosition);
			
			for (int i = 0; i < controlPointsLocal.Count; i++){
				int[] p = terrainCordsToBitmap(terData,controlPointsLocal[i]);
				if (p[0] < 0 || p[1] < 0 || p[0] >= Tw || p[1] >= Th){
					Debug.LogError("The start point or the end point was out of bounds. Make sure the gizmo is over the terrain before setting the start and end points." +
						           "Note: that sometimes Unity does not update the collider after changing settings in the 'Set Resolution' dialog. Entering play mode should reset the collider.");
					return;
				}
			}

			int Sx = (int) Mathf.Floor((Tw / Ts.z) * brushSize); //the x and z are mixed up intentionally
			int Sz = (int) Mathf.Floor((Th / Ts.x) * brushSize); //the x and z are mixed up intentionally
						

			
			float[,] heightMapAll = terData.GetHeights(0, 0, Tw, Th);
			
			//calculate plane. n is plane normal, localEnd and localBegin are in plane.
			//plane uses terrain cord system, y coordinate is in terrain height units, not world units.
			for (int i = 0; i < controlPointsLocal.Count; i++){
				int[] p = terrainCordsToBitmap(terData,controlPointsLocal[i]);
				Vector3 v = controlPointsLocal[i];
				v.y = heightMapAll[p[0],p[1]];
				controlPointsLocal[i] = v;
			}
			
			calculateDistBetweenPoints(controlPointsLocal);
			calculateDistBetweenPointsInPixels(controlPointsLocal,terData);			
			float timeIncrement = (float) (brushSampleDensity) / _totalLengthPixels;
			float timeBrushWidth = (float) (brushSize) / _totalLengthPixels; //the width of the brush in parameterized t
			Debug.Log("Sample w " + Sx + " h " + Sz);
			Debug.Log("parameterized brush width " + timeBrushWidth);
			if (timeBrushWidth > .5f) timeBrushWidth = .5f;
			
			if (VERBOSE){
				for (int i = 0; i < controlPointsLocal.Count; i++){
					int[] pi = terrainCordsToBitmap(terData,controlPointsLocal[i]);
					float[] temp = bitmapCordsToTerrain(terData,pi[0],pi[1]);
					Debug.Log(i + " Local control Pos:" + controlPointsLocal[i]);
					Debug.Log(i + " pixel begin coord:" + pi[0] + " " + pi[0]);
					Debug.Log(i + " Local begin Pos Rev Transformed:" + temp[0] + " " + temp[1]);						
				}
				Debug.Log("parameterized brush width " + timeBrushWidth);
			}			
			StringBuilder sb = new StringBuilder();	
			for(float t = 0.0f; t <= 1.0f; t += timeIncrement){
				Ray psamp = parameterizedLine(t,controlPointsLocal);  //localBeginPosition + t * (v1);
				Vector3 n = Vector3.Cross(new Vector3(-psamp.direction.z, 0.0f, psamp.direction.x), psamp.direction);
				n.Normalize();
				if (spacingJitter > 0f){
					float tt = 2f*Mathf.PI*UnityEngine.Random.value;
					float u = UnityEngine.Random.value+UnityEngine.Random.value;
					float r;
					if (u > 1f) {
						r = 2f-u; 
					} else { 
						r = u;
					}
					r *= spacingJitter * brushSize;
					Vector3 randV = new Vector3(r*Mathf.Cos(tt), 0f, r*Mathf.Sin(tt));
					if (VERBOSE) Debug.Log("jittering by " + randV + " dir " + psamp.direction + " n " + n);
					Plane p = new Plane(n,psamp.origin);
					float ttt;
					Ray rr = new Ray(psamp.origin + randV,Vector3.up);
					if (p.Raycast(rr,out ttt)){
						Vector3 vv = rr.origin + rr.direction * ttt;
						psamp.origin = vv;
					}
				}

				Plane beginClipPlane;
				Plane endClipPlane;
				beginClipPlane = new Plane((controlPointsLocal[0]-controlPointsLocal[1]).normalized, controlPointsLocal[0]);
				endClipPlane = new Plane((controlPointsLocal[controlPointsLocal.Count - 1]-controlPointsLocal[controlPointsLocal.Count - 2]).normalized, controlPointsLocal[controlPointsLocal.Count - 1]);
				int[] pv = terrainCordsToBitmap(terData, psamp.origin);
				Px = pv[0] - Sx/2;
				Py = pv[1] - Sz/2; 
				 
				float[,] heightMap = new float[Sx,Sz]; //little brush size heightmap
				for (int i = 0; i < Sx; i ++){
					for (int j = 0; j <  Sz; j++){
						if (Px + i >= 0 && Py + j >=0 && Px + i < Tw && Py + j < Th) {
							heightMap[i,j] = heightMapAll[Px + i,Py + j] ;
						} else {
							heightMap[i,j] = 0;
						}
					}
				}				
				
				// build a perfect ramp that is size of brush
				float[,] erodedheightMap = (float[,]) heightMap.Clone();
				for (int Tx = 0; Tx < Sx; Tx++) {
					for (int Tz = 0; Tz < Sz; Tz++) {
						float[] My = bitmapCordsToTerrain(terData, (Px + Tx), (Py + Tz));
						//don't ramp points before begin position or after end position
						Vector3 myV = new Vector3(My[0],0f,My[1]);
						Boolean clipThisPoint = false;
						
						if (beginClipPlane.GetSide(myV) && t < timeBrushWidth/2f){
							clipThisPoint = true;
						} else if (endClipPlane.GetSide(myV) && t > 1f - timeBrushWidth/2f){
							clipThisPoint = true;
						}
						
						if (!clipThisPoint){
							//do raycast from zero straight against slope
							Plane p = new Plane(n,psamp.origin);
							Ray r = new Ray(myV,Vector3.up);
							float tt;
							if (!p.Raycast(r, out tt))continue;
//							sb.AppendFormat("n{0} psamp{1} r{2} t{3}\n",n,psamp.ToString("f5"),r,t);
							erodedheightMap[Tx,Tz] = r.origin.y + r.direction.y * tt;
//							Debug.Log("hm " + erodedheightMap[Tx,Tz] + " " +  heightMap[Tx,Tz]);
						}
					}
				}
			
				// Blend it to the terrain object
				float sampleRadius = Mathf.Min(Sz/2.0f, Sx/2.0f);
				for (int Tx = 0; Tx < Sx; Tx++) {
					for (int Ty = 0; Ty < Sz; Ty++) {
						float newHeightAtPoint = erodedheightMap[Tx, Ty];
						float oldHeightAtPoint = heightMap[Tx, Ty];
						float distanceFromCenter = Vector2.Distance(new Vector2(Tx, Ty), new Vector2(sampleRadius, sampleRadius));
						float weightAtPoint = (1.0f - distanceFromCenter / sampleRadius ) / brushSoftness;
						if (weightAtPoint < 0.0f) {
							weightAtPoint = 0.0f;
						} else if (weightAtPoint > 1.0f) {
							weightAtPoint = 1.0f;
						}
						weightAtPoint *= brushOpacity;
						float blendedHeightAtPoint = (newHeightAtPoint * weightAtPoint) + (oldHeightAtPoint * (1.0f - weightAtPoint));
						heightMap[Tx, Ty] = blendedHeightAtPoint;
					}
				}
				
				for (int i = 0; i < Sx; i ++){
					for (int j = 0; j <  Sz; j++){
						if (Px + i >= 0 && Py + j >=0 && Px + i < Tw && Py + j < Th) {
							heightMapAll[Px + i,Py + j] = heightMap[i,j];
						}
					}
				}
			}
			Debug.Log(sb);
			terData.SetHeights(0,0,heightMapAll);
			//beginRamp = endRamp;
		} catch (Exception e) {
		 	Debug.LogError("A brush error occurred: "+e);
		}			
	}
	
	void calculateDistBetweenPoints(List<Vector3> cps){
		_distBetweenPoints.Clear();
		_totalLength = 0;
		for (int i = 1; i < cps.Count; i++){
			_distBetweenPoints.Add((cps[i] - cps[i-1]).magnitude);			
			_totalLength += _distBetweenPoints[_distBetweenPoints.Count - 1];			
		}
	}
	
	void calculateDistBetweenPointsInPixels(List<Vector3> cps, TerrainData terData){
		_totalLengthPixels = 0;
		int[] pi = terrainCordsToBitmap(terData,cps[0]);
		for (int i = 1; i < cps.Count; i++){
			int[] pf = terrainCordsToBitmap(terData,cps[i]);		
        	_totalLengthPixels += Mathf.Sqrt((pf[0]-pi[0])*(pf[0]-pi[0]) + (pf[1]-pi[1])*(pf[1]-pi[1]));
			pi = pf;		
		}
	}
	
	Ray parameterizedLine(float t, List<Vector3> cps, StringBuilder sb=null){
		if (cps.Count < 2){
			Debug.LogError("Less than two control points.");
			return new Ray();
		}
		if (t < 0.0f) t = 0f;
		if (t >= 1.0f) t = 1f;
		
		//control points can have differnt spacing between them
		//adjust t acording to this spacing.
		
		
		//invent two extra control one at beginning one at end extensions of line
		Vector3[] nControlPoints = new Vector3[cps.Count + 2];
		for (int i = 0; i < cps.Count; i++){
			nControlPoints[i+1] = cps[i];	
		}
		nControlPoints[0] = 2f*cps[0] - cps[1];
		nControlPoints[nControlPoints.Length-1] = 2f*cps[cps.Count-1] - cps[cps.Count-2];
		
		//int numSeg = nControlPoints.Length - 3; //don't count first and last
		float distAlongPath = t * _totalLength;
		
		int cpIdx = 0;
		float nt = 0f;
		bool foundSeg = false;
		float distSoFar = 0f;
		while(!foundSeg){
			if (distSoFar + _distBetweenPoints[cpIdx] < distAlongPath){
				distSoFar += _distBetweenPoints[cpIdx];	
				if (cpIdx < controlPoints.Count - 2){
					cpIdx ++;
				} else { //cpIdx == contorlPoints.Count - 2
					foundSeg = true;
					nt = 1.0f;
				}
			} else {
				foundSeg = true;
				nt = (distAlongPath - distSoFar) / _distBetweenPoints[cpIdx];
			}
		}
		
		if (cpIdx >= controlPoints.Count - 1) cpIdx--;
		if (nt > 1f) nt = 1f;
		cpIdx += 1; //add one to account for extra point we added at beginning
		if (cpIdx + 2 > nControlPoints.Length - 1) Debug.LogError("Off end=" + t);
		if (sb != null) sb.AppendFormat("t={0} cpIdx={1} nt={2}\n",t,cpIdx,nt);
		Vector3 p0 = nControlPoints[cpIdx - 1];
		Vector3 p1 = nControlPoints[cpIdx + 0];
		Vector3 p2 = nControlPoints[cpIdx + 1];
		Vector3 p3 = nControlPoints[cpIdx + 2];
		float t2 = nt*nt;
		float t3 = nt*nt*nt;
		Vector3 q = 0.5f *( (2f * p1) +
		 	(-p0 + p2) * nt +
			(2f*p0 - 5f*p1 + 4f*p2 - p3) * t2 +
			(-p0 + 3f*p1- 3f*p2 + p3) * t3);
		
		Vector3 qd = 0.5f * ((-p0 + p2) + 
	          nt*(4.0f*p0 - 10.0f*p1 + 8.0f*p2 - 2.0f*p3) +
	          t2*(-3.0f*p0 + 9.0f*p1- 9.0f*p2 + 3.0f*p3));
		
		Ray r = ( new Ray(q, qd.normalized));
		return r;
		
	}
}
