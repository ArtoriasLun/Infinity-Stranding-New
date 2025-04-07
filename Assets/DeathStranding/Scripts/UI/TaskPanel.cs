using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ALUNGAMES
{
    public class TaskPanel : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        // 移除这些引用，将通过GameController.Instance获取
        //[SerializeField] private PlayerController playerController;
        //[SerializeField] private CityManager cityManager;
        
        private VisualElement root;
        private VisualElement taskPanel;
        
        private void OnEnable()
        {
            Initialize();
            
            // 订阅玩家移动事件，更新任务面板
            GameController.Instance.PlayerController.OnPlayerMoved += UpdateTaskPanel;
        }
        
        private void OnDisable()
        {
            // 取消订阅事件
            if (GameController.Instance != null && GameController.Instance.PlayerController != null)
            {
                GameController.Instance.PlayerController.OnPlayerMoved -= UpdateTaskPanel;
            }
        }
        
        // 初始化
        public void Initialize()
        {
            if (uiDocument == null) return;
            
            root = uiDocument.rootVisualElement;
            
            // 获取任务面板元素
            taskPanel = root.Q<VisualElement>("task-panel");
            
            // 初始更新任务面板
            UpdateTaskPanel();
        }
        
        // 更新任务面板
        public void UpdateTaskPanel()
        {
            if (taskPanel == null) return;
            
            var playerController = GameController.Instance.PlayerController;
            var cityManager = GameController.Instance.CityManager;
            
            if (playerController == null || cityManager == null)
            {
                Debug.LogError("UpdateTaskPanel: playerController或cityManager为null");
                return;
            }
            
            taskPanel.Clear();
            
            // 添加标题
            var titleLabel = new Label("ORDERS");
            titleLabel.AddToClassList("panel-title");
            titleLabel.style.color = new StyleColor(Color.green);
            taskPanel.Add(titleLabel);
            
            List<Task> tasks = playerController.Tasks;
            
            if (tasks.Count == 0)
            {
                var noTasksLabel = new Label("No active tasks");
                noTasksLabel.AddToClassList("task-item");
                taskPanel.Add(noTasksLabel);
            }
            else
            {
                foreach (Task task in tasks)
                {
                    var taskElement = new VisualElement();
                    taskElement.AddToClassList("task-item");
                    
                    string sourcePos = $"{task.Source.x},{task.Source.y}";
                    string destPos = $"{task.Destination.x},{task.Destination.y}";
                    
                    string sourceSymbol = cityManager.GetCitySymbols().TryGetValue(sourcePos, out string source) ? source : "?";
                    string destSymbol = cityManager.GetCitySymbols().TryGetValue(destPos, out string dest) ? dest : "?";
                    
                    var taskLabel = new Label($"Task: From [{sourceSymbol}] to [{destSymbol}] - Cargo: {task.CargoAmount}");
                    taskElement.Add(taskLabel);
                    
                    taskPanel.Add(taskElement);
                }
            }
        }
    }
} 