using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    TextMeshProUGUI loadingText;

    private void Start()
    {
        loadingText = GetComponent<TextMeshProUGUI>();
        StartCoroutine(ChangeText());
    }

    IEnumerator ChangeText()
    {
        yield return new WaitForSeconds(0.2f);

        if(loadingText.text.Length > 9)
            loadingText.text = "Loading";
        else
            loadingText.text += ".";

        StartCoroutine(ChangeText());
    }
}
