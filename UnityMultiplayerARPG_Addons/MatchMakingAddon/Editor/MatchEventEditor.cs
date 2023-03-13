using MarkupAttributes.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(MatchEvents)), CanEditMultipleObjects]
    public class MatchEventEditor : MarkedUpEditor
    {
    }
}