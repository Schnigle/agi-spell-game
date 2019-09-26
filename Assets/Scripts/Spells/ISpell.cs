﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpell
{
    GestureRecognition.Gesture SpellGesture { get; }
    void UnleashSpell();
    void OnAimStart();
    void OnAimEnd();
}