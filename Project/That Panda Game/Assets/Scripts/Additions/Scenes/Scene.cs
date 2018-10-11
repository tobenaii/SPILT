﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene
{
    protected SceneManager _sceneManager;
    protected UiManager _uiManager;

    public Scene(SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        _uiManager = GameObject.Find("UiManager").GetComponent<UiManager>();
    }

    public virtual void Initialize() { }
    public virtual void Cleanup() { }

    public virtual bool IntroTransition() { return true; }

    public virtual bool OutroTransition() { return true; }

    public virtual void Update()
    {
        foreach (User user in SceneManager.Users)
        {
            if (user != null)
                user.Update();
        }
    }
    public virtual void FixedUpdate()
    {
        foreach (User user in SceneManager.Users)
        {
            if (user != null)
                user.FixedUpdate();
        }
    }
}
