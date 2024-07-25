using System;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour{
    public static UiManager Instance;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
        }
        
        Instance = this;
    }
    
    public Image speedVignette;
    
    public Sprite sprintFillSprite;
       
    public Image[] sprintChargesImages;
    [NonSerialized] public Image[] sprintFillImages;
    // public Sprite sprintSprite;
    // public Sprite[] srpintChargeSprites;
    
    private void Start(){
        sprintFillImages = new Image[sprintChargesImages.Length];
        for (int i = 0; i < sprintChargesImages.Length; i++){
            var obj = new GameObject("FillImage");
            obj.transform.parent = sprintChargesImages[i].transform;
            obj.transform.localScale *= 2;
            Image img = obj.AddComponent<Image>();
            img.transform.localPosition = Vector3.zero;
            img.sprite = sprintFillSprite;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Right;
            img.fillAmount = (float)i / (float)sprintChargesImages.Length;
            sprintFillImages[i] = img;
        }
    }
}
