using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowLevelName : MonoBehaviour
{
    private Text _text;
    private string _levelName;

    void Awake()
    {
        _levelName = SceneManager.GetActiveScene().name;
        _text = GetComponent<Text>();
        _text.text = _levelName;
    }
}
