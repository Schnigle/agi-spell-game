﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Spellbook : MonoBehaviour
{
    [SerializeField]
    MeshRenderer page1;
    [SerializeField]
    SkinnedMeshRenderer page2;
    [SerializeField]
    SkinnedMeshRenderer page3;
    [SerializeField]
    MeshRenderer page4;
    [SerializeField]
    float pageTurnRate = 1.5f;
    float flipPageProgress = 0;
    Vector3 flipPageOriginalScale;

    public List<Texture> spellPageTextures = new List<Texture>();
    int visibleLookup = 0;

    float inputStartPosition;
    new Camera camera;

    bool finishTurnForward;
    bool finishTurnBack;
    float contLookup;
    float contLookupWithInput;
    bool wasInputActive;
    float targetPage;
    bool VRIsActive;

    // Start is called before the first frame update
    void Start()
    {
        VRIsActive = Valve.VR.SteamVR.active;
        camera = Camera.main;
        UpdateMaterialScales();
        UpdatePageTextures();
    }

    void UpdateMaterialScales()
    {
        page1.material.mainTextureScale = new Vector2(1, 1);
        page2.material.mainTextureScale = new Vector2(-1, 1);
        page3.material.mainTextureScale = new Vector2(1, 1);
        page4.material.mainTextureScale = new Vector2(-1, 1);
    }

    void UpdatePageTextures()
    {
        int nLookups = Mathf.CeilToInt(spellPageTextures.Count / 2f);
        Texture tex1 = spellPageTextures.Count > visibleLookup * 2? spellPageTextures[visibleLookup * 2] : null;
        Texture tex2 = spellPageTextures.Count > visibleLookup * 2 + 1? spellPageTextures[visibleLookup * 2 + 1] : null;
        Texture tex3 = spellPageTextures.Count > visibleLookup * 2 + 2? spellPageTextures[visibleLookup * 2 + 2] : null;
        Texture tex4 = spellPageTextures.Count > visibleLookup * 2 + 3? spellPageTextures[visibleLookup * 2 + 3] : null;
        if (flipPageProgress == 0)
        {
            page1.material.mainTexture = tex1;
            page4.material.mainTexture = tex2;
            page2.gameObject.SetActive(false);
            page3.gameObject.SetActive(false);
        }
        else
        {
            page2.gameObject.SetActive(true);
            page3.gameObject.SetActive(true);
            page1.material.mainTexture = tex1;
            page2.material.mainTexture = tex2;
            page3.material.mainTexture = tex3;
            page4.material.mainTexture = tex4;
        }
    }

    float GetInputValue()
    {
        if (VRIsActive)
        {
            Vector2 trackpadPos = SteamVR_Input.GetVector2("Trackpad", SteamVR_Input_Sources.LeftHand);
            return trackpadPos.x * pageTurnRate;
        }
        else
        {
            return (Input.mousePosition.x / Screen.width) * pageTurnRate;
        }
    }

    bool IsInputActive()
    {
        if (VRIsActive)
        {
            return SteamVR_Input.GetState("TrackpadActive", SteamVR_Input_Sources.LeftHand);
        }
        else
        {
            return Input.GetMouseButton(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int nLookups = Mathf.CeilToInt(spellPageTextures.Count / 2f);
        float inputDelta = 0;
        bool inputActive = IsInputActive();
        bool inputDown = inputActive && !wasInputActive;
        bool inputUp = wasInputActive && !inputActive;
        wasInputActive = inputActive;
        // Press
        if (inputDown)
        {
            inputStartPosition = GetInputValue();
        }
        // Holding
        if (inputActive)
        {
            inputDelta = Mathf.Clamp(-(GetInputValue() - inputStartPosition), -1f, 1f);
            // inputDelta = -(GetInputValue() - inputStartPosition);
            contLookupWithInput = Mathf.Clamp(contLookup + inputDelta, 0, nLookups-1);
            visibleLookup = Mathf.FloorToInt(contLookupWithInput);
            flipPageProgress = contLookupWithInput % 1;
        }
        // Release
        if (inputUp)
        {
            contLookup = contLookupWithInput;
        }
        // Not holding
        if (!inputActive)
        {
            targetPage = Mathf.Round(contLookup);
            contLookup = Mathf.Lerp(contLookup, targetPage, Time.deltaTime * 10);
            if (contLookup < 0.01f)
            {
                contLookup = 0;
            }
            visibleLookup = Mathf.FloorToInt(contLookup);
            flipPageProgress = contLookup % 1;
        }
        var sinFlip = Mathf.Sin((1-flipPageProgress)*Mathf.PI/2);
        var blendValue = sinFlip * sinFlip;
        // var blendValue = 1-flipPageProgress;
        if (page2 && page4)
        {
            page2.SetBlendShapeWeight(0, blendValue * 100);
            page2.SetBlendShapeWeight(1, Mathf.Sin(blendValue * Mathf.PI) * 100);
            page3.SetBlendShapeWeight(0, blendValue * 100);
            page3.SetBlendShapeWeight(1, Mathf.Sin(blendValue * Mathf.PI) * 100);
        }
        UpdatePageTextures();
    }
}