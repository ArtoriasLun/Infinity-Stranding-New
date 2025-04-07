using UnityEngine;
using UnityEngine.UIElements;

namespace ALUNGAMES
{
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        // 移除这些引用，将通过GameController.Instance获取
        //[SerializeField] private PlayerController playerController;
        //[SerializeField] private DeathStrandingConfig gameConfig;
        
        private VisualElement root;
        
        // HUD元素
        private Label cargoValue;
        private Label bitcoinValue;
        private Label strainValue;
        private VisualElement strainFill;
        
        private void OnEnable()
        {
            Initialize();
            
            // 订阅玩家移动事件，更新HUD
            GameController.Instance.PlayerController.OnPlayerMoved += UpdateHUD;
        }
        
        private void OnDisable()
        {
            // 取消订阅事件
            if (GameController.Instance != null && GameController.Instance.PlayerController != null)
            {
                GameController.Instance.PlayerController.OnPlayerMoved -= UpdateHUD;
            }
        }
        
        // 初始化
        public void Initialize()
        {
            if (uiDocument == null) return;
            
            root = uiDocument.rootVisualElement;
            
            // 获取HUD元素
            cargoValue = root.Q<Label>("cargo-value");
            bitcoinValue = root.Q<Label>("bitcoin-value");
            strainValue = root.Q<Label>("strain-value");
            strainFill = root.Q<VisualElement>("strain-fill");
            
            // 初始更新HUD
            UpdateHUD();
        }
        
        // 更新状态HUD
        public void UpdateHUD()
        {
            var playerController = GameController.Instance.PlayerController;
            var gameConfig = GameController.Instance.GameConfig;
            
            if (playerController == null)
            {
                Debug.LogError("UpdateHUD: playerController is null!");
                return;
            }
            
            // 确保有默认值，以防gameConfig为null
            int maxCargo = gameConfig != null ? gameConfig.maxCargo : 3;
            int maxStrain = gameConfig != null ? gameConfig.maxStrain : 100;
            
            if (cargoValue != null)
                cargoValue.text = $"{playerController.CarriedCargo}/{maxCargo}";
                
            if (bitcoinValue != null)
                bitcoinValue.text = $"{playerController.Bitcoin:F2} ₿";
                
            if (strainValue != null)
                strainValue.text = $"{playerController.Strain}/{maxStrain}";
                
            if (strainFill != null)
            {
                float strainPercentage = (float)playerController.Strain / maxStrain * 100f;
                strainFill.style.width = new StyleLength(new Length(strainPercentage, LengthUnit.Percent));
            }
        }
    }
} 