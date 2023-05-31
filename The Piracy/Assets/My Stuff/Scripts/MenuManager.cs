using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI maxPlayersText;
    public Slider maxPlayersSlider;

    public void OnMaxPlayersChange(){
        maxPlayersText.text = maxPlayersSlider.value.ToString();
    }
}
