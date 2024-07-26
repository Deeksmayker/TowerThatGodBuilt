using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static Source.Utils.Utils;

public class UiManager : MonoBehaviour{
    public static UiManager Instance;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
        }
        
        Instance = this;
        
        sprintFillImages = new Image[sprintChargesImages.Length];
        for (int i = 0; i < sprintChargesImages.Length; i++){
            var obj = new GameObject("FillImage");
            obj.transform.parent = sprintChargesImages[i].transform;
            obj.transform.localScale = Vector3.one * 1;
            Image img = obj.AddComponent<Image>();
            img.transform.localPosition = Vector3.zero;
            img.sprite = sprintFillSprite;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Right;
            img.fillAmount = 0;
            sprintFillImages[i] = img;
            
            sprintChargesImages[i].gameObject.SetActive(false);
        }
    }
    
    public Image speedVignette;
    
    public Sprite sprintFillSprite;
       
    public Image[] sprintChargesImages;
    [NonSerialized] public Image[] sprintFillImages;
    // public Sprite sprintSprite;
    // public Sprite[] srpintChargeSprites;
    
    private float _killingSprintCountdown;
    private int _killingSprintIndex;
    
    private void Update(){
        UpdateKillingSprint(Time.deltaTime);
    }
    
    private void UpdateKillingSprint(float dt){
        if (_killingSprintCountdown > 0){
            _killingSprintCountdown -= dt;
            
            if (_killingSprintCountdown <= 0){
                sprintChargesImages[_killingSprintIndex].gameObject.SetActive(false);
                return;
            }
            
            Image img = sprintFillImages[_killingSprintIndex];
            float t = 1f - (_killingSprintCountdown / _killingSprintTime);
            Color color = img.color;
            color.a = Lerp(.4f, .7f, t * t);
            img.color = color;
            
            img.transform.localScale = Vector3.Lerp(Vector3.one * 1f, Vector3.one * 1.3f, EaseOutCubic(t));
            
            //main sprint image
            Color mainColor = sprintChargesImages[_killingSprintIndex].color;
            mainColor.a = Lerp(.7f, 0f, t * t * t);
            sprintChargesImages[_killingSprintIndex].color = mainColor;
        }
    }
    
    public void SetSprintCharges(int count){
        for (int i = 0; i < count; i++){        
            sprintChargesImages[i].gameObject.SetActive(true);
        }
    }
    
    private float _killingSprintTime = 0.3f;
    public void KillSprintCharge(int index){
        //sprintChargesImages[index].gameObject.SetActive(false);        
        _killingSprintCountdown = _killingSprintTime;
        _killingSprintIndex = index;
    }
    
    public void SetSprintChargeProgress(int index, float t){
        Image fImg = sprintFillImages[index];
        fImg.fillAmount = t;
        Color fColor = fImg.color;
        fColor.a = Lerp(.1f, .4f, t);
        fImg.color = fColor;
        
        Color color = sprintChargesImages[index].color;
        color.a = Lerp(1f, 0.7f, t);
        sprintChargesImages[index].color = color;
        //sprintFillImages[index] = img;
    }
}
