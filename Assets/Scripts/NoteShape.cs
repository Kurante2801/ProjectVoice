using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteShape
{
    Diamond, Circle, Hexagon
}

public static class NoteShapeExtensions
{
    public static string GetLocalized(this NoteShape shape) => $"NOTESHAPE_{shape.ToString().ToUpper()}".Get();
}
