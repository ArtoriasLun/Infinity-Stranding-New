using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ALUNGAMES
{
    public class TaskManager : MonoBehaviour
    {
        private List<Task> tasks = new List<Task>();
        
        // 生成随机任务
        public void GenerateRandomTask()
        {
            // 通过GameController.Instance获取依赖组件
            PlayerController playerController = GameController.Instance.PlayerController;
            CityManager cityManager = GameController.Instance.CityManager;
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            // 检查玩家是否已达最大货物容量
            if (playerController.CarriedCargo >= gameConfig.maxCargo)
                return;
                
            List<City> cities = cityManager.GetCities();
            if (cities.Count < 2)
                return; // 需要至少两个城市
            
            // 随机选择源城市和目标城市（确保不同）
            int sourceIndex = Random.Range(0, cities.Count);
            int destIndex;
            do
            {
                destIndex = Random.Range(0, cities.Count);
            } while (destIndex == sourceIndex);
            
            City sourceCity = cities[sourceIndex];
            City destCity = cities[destIndex];
            
            // 创建新任务
            Task newTask = new Task(
                $"Deliver cargo from {sourceCity.Name} to {destCity.Name}",
                sourceCity.Position,
                destCity.Position,
                1,
                gameConfig.tasks.bitcoinReward
            );
            
            tasks.Add(newTask);
            playerController.AddTask(newTask);
            
            Debug.Log($"New task generated: {newTask.Description}");
        }
        
        // 检查任务是否可以完成
        public void CheckTaskCompletion()
        {
            // 通过GameController.Instance获取PlayerController
            PlayerController playerController = GameController.Instance.PlayerController;
            
            int currentWorldX = playerController.GetCurrentWorldX();
            int currentWorldY = playerController.GetCurrentWorldY();
            
            foreach (Task task in new List<Task>(tasks)) // 复制列表以安全遍历
            {
                if (task.Destination.x == currentWorldX && task.Destination.y == currentWorldY)
                {
                    // 任务可以在此城市完成
                    if (IsPlayerInDeliveryPoint())
                    {
                        CompleteTask(task);
                    }
                }
            }
        }
        
        // 检查玩家是否在交付点
        private bool IsPlayerInDeliveryPoint()
        {
            // 简化检查，实际应根据玩家在城市内的具体位置确定
            return true;
        }
        
        // 完成任务
        private void CompleteTask(Task task)
        {
            // 通过GameController.Instance获取PlayerController
            GameController.Instance.PlayerController.CompleteTask(task);
            tasks.Remove(task);
            
            Debug.Log($"Task completed: {task.Description}");
        }
        
        // 获取当前任务列表
        public List<Task> GetTasks()
        {
            return new List<Task>(tasks); // 返回副本
        }
        
        // 从任务列表中移除任务
        public void RemoveTask(Task task)
        {
            tasks.Remove(task);
        }
        
        // 清空所有任务
        public void ClearTasks()
        {
            tasks.Clear();
        }
    }
} 