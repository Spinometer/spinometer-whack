﻿using Drawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer.SpinalAlignmentVisualizer
{
  public class SpinalAlignmentVisualizerStickFigure : MonoBehaviour
  {
    [SerializeField] private Transform _avatar_skeleton;
    [SerializeField] private Transform _refS;
    [SerializeField] private Transform _refL3;
    [SerializeField] private Transform _refT12;
    [SerializeField] private Transform _refT8;
    [SerializeField] private Transform _refT3;
    [SerializeField] private Transform _refC7;
    [SerializeField] private Transform _refC2;

    private float _dist_S_L3 = 0f;
    private float _dist_L3_T12 = 0f;
    private float _dist_T12_T8 = 0f;
    private float _dist_T8_T3 = 0f;
    private float _dist_T3_C7 = 0f;
    private float _dist_C7_C2 = 0f;

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _alignmentValueLabelPrototype;

    private VisualElement _alignmentValueLabelContainer;
    private Label[] _alignmentValueLabelElements = null;

    private bool _showAlignmentValues;
    public bool ShowAlignmentValues
    {
      get => _showAlignmentValues;
      set
      {
        _showAlignmentValues = value;
        foreach (var el in _alignmentValueLabelElements)
          el.visible = false;
      }
    }

    public bool SmallScreenMode { get; set; }

    private void Awake()
    {
      GrabSegmentLengths();

      /*
       _dist_S_L3 = 0.066f;
       _dist_L3_T12 = 0.093f;
       _dist_T12_T8 = 0.100f;
       _dist_T8_T3 = 0.106f;
       _dist_T3_C7 = 0.086f;
       _dist_C7_C2 = 0.086f;
       _dist_C2_EyePost = 0.080f;
       _dist_EyePost_EyeAnt = 0.050f;
      */

      //

      _alignmentValueLabelContainer = _uiDocument.rootVisualElement.Q<VisualElement>("alignment-values");
      _alignmentValueLabelElements = new Label[16]; // should be enough
      for (var i = 0; i < _alignmentValueLabelElements.Length; i++) {
        var el = _alignmentValueLabelPrototype.Instantiate()[0] as Label;
        _alignmentValueLabelContainer.Add(el);
        _alignmentValueLabelElements[i] = el;
        el.visible = false;
        el.text = $"{i}";
        var pos = Camera.main.WorldToScreenPoint(new Vector3(0f, 0.1f * i, 0f));
        el.style.left = pos.x;
        el.style.top = pos.y;
      }
    }

    private void GrabSegmentLengths()
    {
      _dist_S_L3 = (_refL3.position - _refS.position).magnitude;
      _dist_L3_T12 = (_refT12.position - _refL3.position).magnitude;
      _dist_T12_T8 = (_refT8.position - _refT12.position).magnitude;
      _dist_T8_T3 = (_refT3.position - _refT8.position).magnitude;
      _dist_T3_C7 = (_refC7.position - _refT3.position).magnitude;
      _dist_C7_C2 = (_refC2.position - _refC7.position).magnitude;
    }

    public void DrawAlignment(SpinalAlignment.SpinalAlignment spinalAlignment, bool verbose, bool onSide, float face_dist, float face_pitch)
    {
      // FIXME:  pitch and dist should not be here
      if (!spinalAlignment.absoluteAngles.ContainsKey(SpinalAlignment.SpinalAlignment.AbsoluteAngleId.S))
        return;      

      GrabSegmentLengths();

      Vector3 Dpos(float dist, float angle)
      {
        angle *= Mathf.Deg2Rad;
        return dist * new Vector3(-Mathf.Cos(angle), Mathf.Sin(angle), 0f);
      }

      Vector3 DrawSegment_(Vector3 pos0, Vector3 dpos, bool draw = true)
      {
        Vector3 pos1 = pos0 + dpos;
        if (draw) {
          var offset = 1.01f * Vector3.back + (onSide ? (0.5f * Vector3.right) : Vector3.zero); // FIXME: scale
          var normal = Vector3.back;
          using (Draw.ingame.WithLineWidth(SmallScreenMode ? 3f : 1f)) {
            Draw.ingame.Line(pos1 + offset, pos0 + offset);
            Draw.ingame.Circle(pos1 + offset, normal, 0.01f);
          }
        }
        return pos1;
      }

      Vector3 DrawSegment(Vector3 pos0, float dist, SpinalAlignment.SpinalAlignment.AbsoluteAngleId id, bool draw = true)
      {
        // dist *= _avatar_skeleton.localScale.x;
        float angle = spinalAlignment.absoluteAngles[id];
        Vector3 pos1 = DrawSegment_(pos0, Dpos(dist, angle), draw);
        return pos1;
      }

      void DrawAngle_(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                      float normalCenter, float normalHalfWidth,
                      float angle,
                      string label, Vector2 labelOffset, Color color, int n,
                      bool shortestArc = false)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        GrabSegmentLengths();

        var normalMin = normalCenter - normalHalfWidth;
        var normalMax = normalCenter + normalHalfWidth;

        var offset = 1.01f * Vector3.back + (onSide ? (0.5f * Vector3.right) : Vector3.zero); // FIXME: scale
        pos0 += offset;
        pos1 += offset;
        pos2 += offset;
        var normal = Vector3.back;
        pos0 = Vector3.Lerp(pos1, pos0, 0.4f);
        pos2 = Vector3.Lerp(pos1, pos2, 0.4f);
        var vec10 = (pos0 - pos1).normalized;
        var vec12 = (pos2 - pos1).normalized;
        var radius = ((pos0 - pos1).magnitude + (pos2 - pos0).magnitude) * 0.5f * 0.2f;
        using (Draw.ingame.WithColor(color)) {
          if (!verbose) {
            if (n < _alignmentValueLabelElements.Length) {
              var el = _alignmentValueLabelElements[n];
              el.visible = false;
            }
          } else {
            using (Draw.ingame.WithLineWidth(SmallScreenMode ? 5f : 3f)) {
              if (shortestArc)
                Draw.ingame.Arc(pos1, pos1 + vec10 * radius, pos1 + vec12 * radius);
              else
                Util.Drawing.DrawArc(pos1, pos1 + vec10 * radius, pos1 + vec12 * radius);
            }

            if (n < _alignmentValueLabelElements.Length) {
              var el = _alignmentValueLabelElements[n];
              var screenPos = Camera.main.WorldToScreenPoint(pos1/* + offset*/);
              var uiPosX = screenPos.x / Screen.width * _alignmentValueLabelContainer.layout.width;
              var uiPosY = (1.0f - screenPos.y / Screen.height) * _alignmentValueLabelContainer.layout.height;
              el.visible = true;
              el.text = $"{label}\n{angle:0.0}";
              el.style.left = uiPosX + labelOffset.x;
              el.style.top = uiPosY + labelOffset.y - 20f;
              el.style.color = color;
              bool withinNormalBound = angle >= normalMin && angle <= normalMax;
              el.style.backgroundColor = withinNormalBound ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0f, 0f, 0.2f);
            }
          }
        }
      }

      void DrawAngle(Vector3 pos0, Vector3 pos1, Vector3 pos2,
                     float normalCenter, float normalHalfWidth,
                     SpinalAlignment.SpinalAlignment.RelativeAngleId id,
                     string label, Vector2 labelOffset, Color color, int n)
      {
        // FIXME:  differentiate or merge verbose and _showAlignmentValues.

        float angle = spinalAlignment.relativeAngles[id];
        DrawAngle_(pos0, pos1, pos2,
                   normalCenter, normalHalfWidth,
                   angle,
                   label, labelOffset, color, n);
      }

      var pos_s = _refS.position;
      var pos_l3 = DrawSegment(pos_s, _dist_S_L3, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.S);
      var pos_t12 = DrawSegment(pos_l3, _dist_L3_T12, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.L3);
      var pos_t8 = DrawSegment(pos_t12, _dist_T12_T8, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.T12);
      var pos_t3 = DrawSegment(pos_t8, _dist_T8_T3, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.T8);
      var pos_c7 = DrawSegment(pos_t3, _dist_T3_C7, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.T3);
      var pos_c2 = DrawSegment(pos_c7, _dist_C7_C2, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.C7);
      // pos0 = NextPos(pos0, _dist_EyePost_EyeAnt, SpinalAlignment.SpinalAlignment.AbsoluteAngleId.EyePost);
      var headJointOffset = new Vector3(-0.086f, 0.102f, 0f); // FIXME: scale 
      var pos_eyepost = pos_c2 + headJointOffset + Quaternion.AngleAxis(-face_pitch, Vector3.forward) * (new Vector3(-0.480f, 0.200f, 0f) - headJointOffset); // FIXME: scale
      var vec_sight = Quaternion.AngleAxis(-face_pitch, Vector3.forward) * Vector3.left * 0.5f; // FIXME: scale
      var off_headCenter = new Vector3(-0.185f, 0.257f, 0f);
      {
        using (Draw.ingame.WithColor(Color.gray)) {
          DrawSegment_(pos_t3 + 0.2f * Vector3.up, -0.2f * Vector3.up, true);
          DrawSegment_(pos_c7 + 0.2f * Vector3.up, -0.2f * Vector3.up, true);
          DrawSegment_(pos_eyepost + 0.5f * Vector3.left, -0.5f * Vector3.left, true);
        }
        DrawSegment_(pos_eyepost, vec_sight, true);
        var normal = Vector3.back;
        var radius = 0.35f;
        var headCenterPos = pos_c2 + headJointOffset + Quaternion.AngleAxis(-face_pitch, Vector3.forward) * (off_headCenter - headJointOffset); // FIXME: scale 
        var offset = 1.01f * Vector3.back + (onSide ? (0.5f * Vector3.right) : Vector3.zero);
        using (Draw.ingame.WithLineWidth(SmallScreenMode ? 3f : 1f)) {
          Draw.ingame.Circle(offset + headCenterPos, normal, radius, Color.white);
        }
      }

      if (!_showAlignmentValues)
        return;

      float length = !verbose ? 0.1f : 0.4f;
      var color0 = new Color(0.5f, 1.0f, 1.0f);
      var color1 = new Color(1.0f, 0.5f, 1.0f);
      int n = 0;
      DrawAngle(pos_c7 + Vector3.up * length,
                pos_c7,
                pos_c2,
                30f, 5f,
                SpinalAlignment.SpinalAlignment.RelativeAngleId.C2_C7_vert_new, "C2_C7_vert", new Vector2(10f, -55f), color0, n++);
      DrawAngle(pos_t3 + Vector3.up * length,
                pos_t3,
                pos_c7,
                40f, 10f,
                SpinalAlignment.SpinalAlignment.RelativeAngleId.C7_T3_vert_new, "C7_T3_vert", new Vector2(10f, -40f), color1, n++);
      // T1_slope
      DrawAngle(pos_c7,
                pos_t3,
                pos_t8,
                150f, 8f,
                SpinalAlignment.SpinalAlignment.RelativeAngleId.C7_T3_T8, "C7_T3_T8", new Vector2(25f, 10f), color0, n++);
      DrawAngle(pos_t3,
                pos_t8,
                pos_t12,
                155f, 1f,
                SpinalAlignment.SpinalAlignment.RelativeAngleId.T3_T8_T12, "T3_T8_T12", new Vector2(20f, 0f), color1, n++);
      DrawAngle(pos_t8,
                pos_t12,
                pos_l3,
                177.7f, 2.5f,
                SpinalAlignment.SpinalAlignment.RelativeAngleId.T8_T12_L3, "T8_T12_L3", new Vector2(20f, 0f), color0, n++);
      DrawAngle(pos_s,
                pos_l3,
                pos_t12,
                172.5f, 1.5f,
                SpinalAlignment.SpinalAlignment.RelativeAngleId.T12_L3_S, "T12_L3_S", new Vector2(30f, 0f), color1, n++);
      {
        DrawAngle_(pos_eyepost + Vector3.left * 2.0f, pos_eyepost, pos_eyepost + vec_sight, // FIXME: scale
                   0f, 180f,
                   face_pitch,
                   "pitch", new Vector2(-80f, -40f), color0, n++, true);
      }


      for (; n < _alignmentValueLabelElements.Length; n++) {
        _alignmentValueLabelElements[n].visible = false;
      }
    }
  }
}
