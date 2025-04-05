const CityModule = {
    citySymbolPool: ["α", "β", "γ", "δ", "ε", "ζ", "η", "θ", "ι", "κ"],
    
    // 基础的空城市布局 - 将动态调整大小
    baseCityLayout: [
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '|', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
      ['.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.'],
    ],
  
    initCities(count, worldMapWidth, worldMapHeight) {
      this.cities = [];
      this.citySymbols = {};
      this.generateCities(count, worldMapWidth, worldMapHeight);
      return { cities: this.cities, citySymbols: this.citySymbols };
    },
  
    generateCities(count, width, height) {
      for (let i = 0; i < count; i++) {
        let x, y;
        let validPosition = false;
        
        // 尝试放置城市的最大次数
        let attempts = 0;
        const maxAttempts = 100;
        
        while (!validPosition && attempts < maxAttempts) {
          attempts++;
          x = Math.floor(Math.random() * width);
          y = Math.floor(Math.random() * height);
          
          // 检查与现有城市的距离
          validPosition = true;
          for (let existingCity of this.cities) {
            // 曼哈顿距离：|x1-x2| + |y1-y2|
            const distance = Math.abs(existingCity.worldX - x) + Math.abs(existingCity.worldY - y);
            if (distance < 4) {
              validPosition = false;
              break;
            }
          }
        }
        
        // 如果找到有效位置，添加城市
        if (validPosition) {
          this.cities.push({ worldX: x, worldY: y });
        } else {
          console.warn(`Could not place city ${i} after ${maxAttempts} attempts. Skipping.`);
        }
      }
      
      // 为每个城市分配唯一符号
      for (let city of this.cities) {
        const key = `${city.worldX},${city.worldY}`;
        let idx = Math.floor(Math.random() * this.citySymbolPool.length);
        this.citySymbols[key] = this.citySymbolPool.splice(idx, 1)[0];
      }
    },
  
    isCityChunk(currentX, currentY) {
      return this.cities.some(c => c.worldX === currentX && c.worldY === currentY);
    },
  
    injectCityLayout(terrain, mapSize) {
      // 复制城市布局
      const layout = JSON.parse(JSON.stringify(this.baseCityLayout));
      
      // 计算放置点
      const layoutH = layout.length;
      const layoutW = layout[0].length;
      const startY = Math.floor((mapSize - layoutH) / 2);
      const startX = Math.floor((mapSize - layoutW) / 2);
      
      // 建筑记录
      const buildings = {};
      
      // 第一步：先放置荒野地形，确保城市外部是荒野而不是灰色
      for (let row = 0; row < layoutH; row++) {
        for (let col = 0; col < layoutW; col++) {
          terrain[startY + row][startX + col] = '.'; // 普通荒野地形
        }
      }
      
      // 定义固定的城市围墙边界
      const minX = startX + 6; 
      const maxX = startX + 24;
      const minY = startY + 6;
      const maxY = startY + 18;
      
      // 直接放置围墙，不再使用wallGrid
      
      // 顶部和底部横墙
      for (let x = minX; x <= maxX; x++) {
        terrain[minY][x] = '#'; // 顶部墙
        terrain[maxY][x] = '#'; // 底部墙
      }
      
      // 左侧和右侧竖墙
      for (let y = minY + 1; y < maxY; y++) {
        terrain[y][minX] = '#'; // 左侧墙
        terrain[y][maxX] = '#'; // 右侧墙
      }
      
      // 设置门的位置
      let rightDoorY = startY + 12;
      let leftDoorY = startY + 14;
      
      // 放置门
      terrain[rightDoorY][maxX] = '|'; // 右侧门
      terrain[leftDoorY][minX] = '|'; // 左侧门
      
      // 在城市内部放置建筑
      const barPoint = BuildingsModule.placeBuilding(terrain, "bar", minX + 4, minY + 5);
      const yardPoint = BuildingsModule.placeBuilding(terrain, "yard", minX + 10, minY + 8);
      const hotelPoint = BuildingsModule.placeBuilding(terrain, "hotel", maxX - 6, minY + 5);
      
      if (barPoint) buildings["bar"] = barPoint;
      if (yardPoint) buildings["yard"] = yardPoint;
      if (hotelPoint) buildings["hotel"] = hotelPoint;
      
      // 在城市墙外添加一些随机树木装饰
      this.addTreesAroundCity(terrain, minX, minY, maxX, maxY, mapSize);
      
      return buildings;
    },
    
    // 在城市墙外添加树木
    addTreesAroundCity(terrain, minX, minY, maxX, maxY, mapSize) {
      // 树木密度 - 值越低，树木越多
      const treeDensity = 0.8; 
      
      // 在城市墙外1-3格范围内添加树木
      for (let y = Math.max(0, minY - 3); y <= Math.min(mapSize - 1, maxY + 3); y++) {
        for (let x = Math.max(0, minX - 3); x <= Math.min(mapSize - 1, maxX + 3); x++) {
          // 只在城市墙外添加树木
          if (x < minX || x > maxX || y < minY || y > maxY) {
            // 随机决定是否放置树木
            if (Math.random() > treeDensity) {
              terrain[y][x] = '✳';
            }
          }
        }
      }
    },
  
    handleCityAction(player, terrain, currentX, currentY, tasks, carriedCargo, bitcoin, strain) {
      const tile = terrain[player.y][player.x];
      
      // 处理不同的交互点
      if (tile === '■' || tile === '□') {
        // 获取最新建筑位置
        let buildings = this.injectCityLayout(terrain, Game.mapSize);
        let actionType = null;
        
        // 检查玩家在哪个建筑中
        for (let key in buildings) {
          if (buildings[key].x === player.x && buildings[key].y === player.y) {
            actionType = buildings[key].type;
            break;
          }
        }
        
        if (actionType) {
          // 委托给BuildingsModule处理具体操作
          return BuildingsModule.handleBuildingAction(
            actionType, 
            currentX, 
            currentY, 
            tasks, 
            carriedCargo, 
            bitcoin, 
            strain,
            this.cities,
            this.citySymbols
          );
        }
      }
      
      return { 
        acted: false, 
        message: "Please stand at a pickup (■), delivery (□), or rest point to interact.",
        tasks, 
        carriedCargo, 
        bitcoin,
        strain
      };
    }
  };