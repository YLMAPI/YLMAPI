using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BoundBoxes_BoundBox : MonoBehaviour {
	
	public Color lineColor = new Color(0f,1f, 0.4f,0.74f);

	private Bounds bound;
	
	private Vector3[] corners;
	
	private Vector3[,] lines;
	
	private MeshFilter[] meshes;
	
	private Vector3 topFrontLeft;
	private Vector3 topFrontRight;
	private Vector3 topBackLeft;
	private Vector3 topBackRight;
	private Vector3 bottomFrontLeft;
	private Vector3 bottomFrontRight;
	private Vector3 bottomBackLeft;
	private Vector3 bottomBackRight;
	

	void Awake () {	
        if (GetComponent<Collider>() == null) {
            Destroy(this);
            return;
        }
        meshes = GetComponentsInChildren<MeshFilter>();
    }
	
	
	void Start () {
		init();
	}
	
	public void init() {
        calculateBounds();
		setPoints();
		setLines();
        Camera.main.transform.GetOrAddComponent<BoundBoxes_drawLines>().setOutlines(lines,lineColor);
	}
	
	void LateUpdate() {
        Camera.main.transform.GetOrAddComponent<BoundBoxes_drawLines>().setOutlines(lines,lineColor);
	}
	
	void calculateBounds() {
        // bound = new Bounds();
        // bound.Encapsulate(GetComponent<BoxCollider>().bounds);

        Collider collider = GetComponent<Collider>();
        bound = new Bounds();
        bound.Encapsulate(collider.bounds);

        Quaternion quat = transform.rotation;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        for (int i = 0; i < meshes.Length; i++) {
            Mesh ms = meshes[i].mesh;
            Vector3 tr = meshes[i].gameObject.transform.position;
            Vector3 ls = meshes[i].gameObject.transform.lossyScale;
            Quaternion lr = meshes[i].gameObject.transform.rotation;
            int vc = ms.vertices.Length;
            for (int j = 0; j < vc; j++) {
                //if (i == 0 && j == 0) {
                //    bound = new Bounds(tr + lr * Vector3.Scale(ls, ms.vertices[j]), Vector3.zero);
                //} else {
                    bound.Encapsulate(tr + lr * Vector3.Scale(ls, ms.vertices[j]));
                //}
            }
        }
        transform.rotation = quat;
    }
	
	void setPoints() {
        Quaternion quat = transform.rotation;
		Vector3 bc = transform.position + quat *(bound.center - transform.position);

		topFrontRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, 1, 1)); 
		topFrontLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, 1, 1)); 
		topBackLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, 1, -1));
		topBackRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, 1, -1)); 
		bottomFrontRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, -1, 1)); 
		bottomFrontLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, -1, 1)); 
		bottomBackLeft = bc +  quat *Vector3.Scale(bound.extents, new Vector3(-1, -1, -1));
		bottomBackRight = bc +  quat *Vector3.Scale(bound.extents, new Vector3(1, -1, -1)); 
		corners = new Vector3[]{topFrontRight,topFrontLeft,topBackLeft,topBackRight,bottomFrontRight,bottomFrontLeft,bottomBackLeft,bottomBackRight};
		
	}
	
	void setLines() {
		
		int i1;
		int linesCount = 12;

		lines = new Vector3[linesCount,2];
		for (int i=0; i<4; i++) {
			i1 = (i+1)%4;//top rectangle
			lines[i,0] = corners[i];
			lines[i,1] = corners[i1];
			//break;
			i1 = i + 4;//vertical lines
			lines[i+4,0] = corners[i];
			lines[i+4,1] = corners[i1];
			//bottom rectangle
			lines[i+8,0] = corners[i1];
			i1 = 4 + (i+1)%4;
			lines[i+8,1] = corners[i1];
		}
	}
	
}
