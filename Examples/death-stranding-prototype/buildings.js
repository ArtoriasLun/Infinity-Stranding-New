const BuildingsModule = {
  // 定义模块化建筑
  buildings: {
    bar: {
      name: "BAR",
      layout: [
        "####",
        "#■.|",
        "####"
      ],
      action: "pickup", // 接任务/收货
      width: 4,
      height: 3
    },
    yard: {
      name: "YARD",
      layout: [
        "####",
        "#□.|",
        "####"
      ],
      action: "delivery", // 交任务/交货
      width: 4,
      height: 3
    },
    hotel: {
      name: "HOTEL",
      layout: [
        "#####",
        "|..■#",
        "#####"
      ],
      action: "rest", // 清除strain进度条
      width: 5,
      height: 3
    }
  },
  
  // 放置建筑并返回互动点坐标
  placeBuilding(terrain, buildingType, startX, startY) {
    const building = this.buildings[buildingType];
    if (!building) return;
    
    // 先放置建筑名称
    const nameX = startX + Math.floor((building.layout[0].length - building.name.length) / 2);
    for (let i = 0; i < building.name.length; i++) {
      terrain[startY - 1][nameX + i] = building.name[i];
    }
    
    // 放置建筑本身
    for (let y = 0; y < building.layout.length; y++) {
      for (let x = 0; x < building.layout[y].length; x++) {
        terrain[startY + y][startX + x] = building.layout[y][x];
      }
    }
    
    // 返回建筑的关键点坐标（比如取货点、交货点）
    for (let y = 0; y < building.layout.length; y++) {
      for (let x = 0; x < building.layout[y].length; x++) {
        if (building.layout[y][x] === '■' || building.layout[y][x] === '□') {
          return { 
            x: startX + x, 
            y: startY + y, 
            type: building.action,
            buildingWidth: building.width,
            buildingHeight: building.height
          };
        }
      }
    }
  },
  
  // 处理建筑互动
  handleBuildingAction(actionType, currentX, currentY, tasks, carriedCargo, bitcoin, strain, cities, citySymbols) {
    if (actionType === "pickup") {
      // 取货点逻辑
      if (carriedCargo >= 3) {
        return { 
          acted: true, 
          message: "Cargo capacity full (3/3)!",
          tasks, 
          carriedCargo, 
          bitcoin,
          strain
        };
      }
      
      let alreadyTaken = tasks.some(task =>
        task.fromCity.worldX === currentX && task.fromCity.worldY === currentY
      );
      
      if (!alreadyTaken) {
        let city = cities.find(c => c.worldX === currentX && c.worldY === currentY);
        let otherCities = cities.filter(c2 => c2.worldX !== city.worldX || c2.worldY !== city.worldY);
        
        if (otherCities.length > 0) {
          let targetCity = otherCities[Math.floor(Math.random() * otherCities.length)];
          tasks.push({ fromCity: city, toCity: targetCity, cargo: 1 });
          carriedCargo++;
          
          return { 
            acted: true, 
            message: `Task received at BAR! Target city: (${targetCity.worldX},${targetCity.worldY})`,
            tasks, 
            carriedCargo, 
            bitcoin,
            strain
          };
        }
      } else {
        return { 
          acted: true, 
          message: "Task for this city already taken!",
          tasks, 
          carriedCargo, 
          bitcoin,
          strain
        };
      }
    } else if (actionType === "rest") {
      // 休息点逻辑 - HOTEL
      return { 
        acted: true, 
        message: "You rested at the HOTEL. Strain reduced to zero!",
        tasks, 
        carriedCargo, 
        bitcoin,
        strain: 0 // 重置strain为0
      };
    } else if (actionType === "delivery") {
      // 交货点逻辑
      let delivered = false;
      
      for (let i = tasks.length - 1; i >= 0; i--) {
        let task = tasks[i];
        if (task.toCity.worldX === currentX && task.toCity.worldY === currentY) {
          bitcoin += task.cargo * 0.33;
          delivered = true;
          tasks.splice(i, 1);
          carriedCargo = Math.max(0, carriedCargo - 1);
          break;
        }
      }
      
      return { 
        acted: true, 
        message: delivered ? "Task delivered successfully, Bitcoin earned!" : "No tasks to deliver here.",
        tasks, 
        carriedCargo, 
        bitcoin,
        strain
      };
    }
    
    return { acted: false, message: "No action available.", tasks, carriedCargo, bitcoin, strain };
  }
}; 