using System.Collections.Generic;
using UnityEngine;

public class ThrowIndicator : MonoBehaviour
{

    [Tooltip("Prefab with a SpriteRenderer representing one javelin / throw token.")]
    [SerializeField] private GameObject javelinIconPrefab;

    [Tooltip("Gap between each icon in world units.")]
    [SerializeField] private float spacing = 0.35f;

    [Tooltip("Uniform scale applied to each spawned icon.")]
    [SerializeField] private float iconScale = 0.25f;

    [Tooltip("Colour applied to icons that have been spent.")]
    [SerializeField] private Color usedColour = new Color(1f, 1f, 1f, 0.25f);

    [Tooltip("If true, spent icons are hidden instead of tinted.")]
    [SerializeField] private bool hideUsed = false;

    private readonly List<SpriteRenderer> _icons = new List<SpriteRenderer>();
    private int _maxThrows;
    private int _throwsRemaining;
    public void Initialise(int maxThrows)
    {
        _maxThrows = maxThrows;
        _throwsRemaining = maxThrows;

        foreach (var icon in _icons)
            if (icon != null) Destroy(icon.gameObject);
        _icons.Clear();

        if (javelinIconPrefab == null)
        {
            Debug.LogWarning("[ThrowIndicator] No javelinIconPrefab assigned.");
            return;
        }

        for (int i = 0; i < maxThrows; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(-i * spacing, 0f, 0f);
            GameObject go = Instantiate(javelinIconPrefab, spawnPos, transform.rotation, transform);
            go.transform.localScale = Vector3.one * iconScale;
            go.transform.right = Vector2.up;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
                _icons.Add(sr);
            else
                Debug.LogWarning("[ThrowIndicator] javelinIconPrefab has no SpriteRenderer.");
        }

        Refresh();
    }
    public void SetThrowsRemaining(int remaining)
    {
        _throwsRemaining = Mathf.Clamp(remaining, 0, _maxThrows);
        Refresh();
    }
    private void Refresh()
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (_icons[i] == null) continue;

            bool isAvailable = i < _throwsRemaining;

            if (hideUsed)
            {
                _icons[i].gameObject.SetActive(isAvailable);
            }
            else
            {
                _icons[i].gameObject.SetActive(true);
                _icons[i].color = isAvailable ? Color.white : usedColour;
            }
        }
    }
}