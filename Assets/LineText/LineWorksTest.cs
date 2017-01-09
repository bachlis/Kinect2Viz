using UnityEngine;
using System.Collections;
using LineWorks;

public class LineWorksTest : MonoBehaviour {

    private LW_Canvas canvas;
    private LW_Stroke masterStroke;
    private LW_Polyline3D[] lines;
    private Vector3[][] linePoints;
    Material mat;


    // Use this for initialization
    void Start () {
        canvas = LW_Canvas.Create(new GameObject(), "ImpactRings", false);
        //linework = LW_Canvas.Create(gameObject, "ImpactRings", false);
        canvas.blendMode = BlendMode.AlphaBlend;
        canvas.featureMode = FeatureMode.Advanced;
        canvas.strokeDrawMode = StrokeDrawMode.Draw2D;
        canvas.joinsAndCapsMode = JoinsAndCapsMode.Shader;
        canvas.gradientsMode = GradientsMode.Vertex;
        canvas.antiAliasingMode = AntiAliasingMode.On;

        masterStroke = LW_Stroke.Create(Color.white, 0.05f);
        masterStroke.paintMode = PaintMode.RadialGradient;
        masterStroke.gradientUnits = GradientUnits.userSpaceOnUse;
        masterStroke.presizeVBO = 100;

        lines = new LW_Polyline3D[1000];
        LW_Stroke stroke = LW_Stroke.Create(Color.white, 1f);
        stroke.linejoin = Linejoin.Break;
        canvas.graphic.styles.Add(stroke);

        linePoints = new Vector3[lines.Length][];
        for (int i = 0; i < lines.Length; i++)
        {
            linePoints[i] = new Vector3[2];
            lines[i] = LW_Polyline3D.Create(linePoints[i], false);
            canvas.graphic.Add(lines[i]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].points[0] = Random.onUnitSphere;
            lines[i].points[1] = Random.onUnitSphere;
            lines[i].SetElementDirty();
        }
    }
}
