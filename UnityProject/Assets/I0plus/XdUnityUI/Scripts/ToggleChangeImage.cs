using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleChangeImage : MonoBehaviour
{
    private Toggle toggle;

    private void Awake()
    {
        toggle = gameObject.GetComponent<Toggle>();
        if (toggle == null) return;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(bool on)
    {
        if (toggle == null || toggle.image == null) return;
        toggle.image.enabled = !on;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (toggle == null) return;
        OnValueChanged(toggle.isOn);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        if (toggle == null) return;
        toggle.onValueChanged.RemoveListener(OnValueChanged);
    }
}