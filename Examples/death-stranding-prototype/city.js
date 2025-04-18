const CityModule = {
    cities: [],
    citySymbols: {},
    worldMap: null,
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
      ['.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '#', '.', '.', '.', '.', '.', '.'],
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
  
    initCities(count, worldWidth, worldHeight, worldMap) {
        this.worldMap = worldMap;
        const { cities, citySymbols } = this.generateCities(count, worldWidth, worldHeight);
        this.cities = cities;
        this.citySymbols = citySymbols;
        return { cities, citySymbols };
    },
  
    generateCities(count, worldWidth, worldHeight) {
        const cities = [];
        const citySymbols = {};
        const usedPositions = new Set();

        while (cities.length < count) {
            const x = Math.floor(Math.random() * worldWidth);
            const y = Math.floor(Math.random() * worldHeight);
            const posKey = `${x},${y}`;

            if (!usedPositions.has(posKey)) {
                usedPositions.add(posKey);
                cities.push({
                    worldX: x,
                    worldY: y,
                    buildings: this.generateCityBuildings()
                });
                // 使用希腊字母池中的符号，确保不越界
                const symbolIndex = Math.min(cities.length - 1, this.citySymbolPool.length - 1);
                citySymbols[posKey] = this.citySymbolPool[symbolIndex];
            }
        }

        return { cities, citySymbols };
    },
  
    generateCityBuildings() {
        const buildings = [];
        const buildingTypes = ['bar', 'yard', 'hotel', 'exchange'];
        
        // 确保每个城市都有yard
        const yardBuilding = BuildingModule.generateBuilding('yard');
        if (yardBuilding) {
            buildings.push(yardBuilding);
        }
        
        // 从剩余建筑类型中随机选择，确保每种类型最多只有一个
        const remainingTypes = buildingTypes.filter(type => type !== 'yard');
        const shuffledTypes = [...remainingTypes].sort(() => Math.random() - 0.5);
        
        // 选择1-3个其他建筑
        const additionalCount = 1 + Math.floor(Math.random() * Math.min(3, remainingTypes.length));
        
        for (let i = 0; i < additionalCount; i++) {
            if (i < shuffledTypes.length) {
                const building = BuildingModule.generateBuilding(shuffledTypes[i]);
                if (building) {
                    buildings.push(building);
                }
            }
        }

        return buildings;
    },
  
    isCityChunk(worldX, worldY) {
        return this.cities.some(city => city.worldX === worldX && city.worldY === worldY);
    },
  
    injectCityLayout(terrain, mapSize) {
        const currentCity = this.cities.find(city => 
            city.worldX === this.worldMap.currentX && 
            city.worldY === this.worldMap.currentY
        );

        if (!currentCity) return;

        // Clear a space for the city
        const citySize = 20;
        const startX = Math.floor((mapSize - citySize) / 2);
        const startY = Math.floor((mapSize - citySize) / 2);
        
        // 添加城市外墙
        for (let x = startX; x < startX + citySize; x++) {
            terrain[startY][x] = '#';  // 上墙
            terrain[startY + citySize - 1][x] = '#';  // 下墙
        }
        
        for (let y = startY; y < startY + citySize; y++) {
            terrain[y][startX] = '#';  // 左墙
            terrain[y][startX + citySize - 1] = '#';  // 右墙
        }
        
        // 在墙上创建四个入口 (四个方向的中间位置)
        const midX = startX + Math.floor(citySize / 2);
        const midY = startY + Math.floor(citySize / 2);
        
        // 右方门
        terrain[midY][startX + citySize - 1] = '|';
        
        // 下方门
        terrain[startY + citySize - 1][midX] = '|';
        
        // 左方门
        terrain[midY][startX] = '|';

        // Place buildings in the city
        let buildingX = startX + 2;
        let buildingY = startY + 2;

        for (const building of currentCity.buildings) {
            // 保存建筑物位置坐标
            building.x = buildingX;
            building.y = buildingY;
            
            // Place building name
            const nameX = buildingX + Math.floor((building.width - building.name.length) / 2);
            for (let i = 0; i < building.name.length; i++) {
                terrain[buildingY - 1][nameX + i] = building.name[i];
            }

            // Place building layout
            for (let y = 0; y < building.height; y++) {
                for (let x = 0; x < building.width; x++) {
                    terrain[buildingY + y][buildingX + x] = building.layout[y][x];
                }
            }

            // Update position for next building
            buildingX += building.width + 2;
            if (buildingX + building.width > startX + citySize) {
                buildingX = startX + 2;
                buildingY += building.height + 2;
            }
        }
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
  
    handleCityAction(player, terrain, worldX, worldY, tasks, carriedCargo, bitcoin, strain) {
        const currentCity = this.cities.find(city => 
            city.worldX === worldX && city.worldY === worldY
        );

        if (!currentCity) return { acted: false, message: "Not in a city." };

        // Find the building the player is in
        const building = this.findBuildingAtPosition(currentCity, player.x, player.y);
        if (!building) return { acted: false, message: "Not in a building." };

        // Get the special point type at player's position
        const pointType = BuildingModule.getSpecialPointType(building, 
            player.x - building.x, 
            player.y - building.y
        );

        if (!pointType) return { acted: false, message: "No action point here." };

        // Handle different point types
        switch (pointType) {
            case 'task':
                return this.handleTaskPoint(tasks, carriedCargo, currentCity);
            case 'delivery':
                return this.handleDeliveryPoint(tasks, carriedCargo, bitcoin);
            case 'rest':
                return this.handleRestPoint(strain);
            default:
                return { acted: false, message: "Unknown action point." };
        }
    },

    // Find building at position
    findBuildingAtPosition(city, x, y) {
        for (const building of city.buildings) {
            if (x >= building.x && x < building.x + building.width &&
                y >= building.y && y < building.y + building.height) {
                return building;
            }
        }
        return null;
    },

    // Handle task point actions
    handleTaskPoint(tasks, carriedCargo, currentCity) {
        if (carriedCargo >= GameConfig.player.maxCargo) {
            return { 
                acted: true, 
                message: `Cargo capacity full (${carriedCargo}/${GameConfig.player.maxCargo})!`,
                tasks, 
                carriedCargo, 
                bitcoin: 0,
                strain: 0
            };
        }

        let alreadyTaken = tasks.some(task =>
            task.fromCity.worldX === currentCity.worldX && 
            task.fromCity.worldY === currentCity.worldY
        );

        if (!alreadyTaken) {
            let otherCities = this.cities.filter(city => 
                city.worldX !== currentCity.worldX || 
                city.worldY !== currentCity.worldY
            );

            if (otherCities.length > 0) {
                let targetCity = otherCities[Math.floor(Math.random() * otherCities.length)];
                tasks.push({ 
                    fromCity: currentCity, 
                    toCity: targetCity, 
                    cargo: 1 
                });
                carriedCargo++;

                return { 
                    acted: true, 
                    message: `Task received! Target city: ${this.citySymbols[`${targetCity.worldX},${targetCity.worldY}`]}`,
                    tasks, 
                    carriedCargo, 
                    bitcoin: 0,
                    strain: 0
                };
            }
        }

        return { 
            acted: true, 
            message: "Task for this city already taken!",
            tasks, 
            carriedCargo, 
            bitcoin: 0,
            strain: 0
        };
    },

    // Handle delivery point actions
    handleDeliveryPoint(tasks, carriedCargo, bitcoin) {
        let delivered = false;
        
        for (let i = tasks.length - 1; i >= 0; i--) {
            let task = tasks[i];
            if (task.toCity.worldX === this.worldMap.currentX && 
                task.toCity.worldY === this.worldMap.currentY) {
                bitcoin += task.cargo * GameConfig.tasks.bitcoinReward;
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
            strain: 0
        };
    },

    // Handle rest point actions
    handleRestPoint(strain) {
        return { 
            acted: true, 
            message: "You rested. Strain reduced to zero!",
            tasks: [], 
            carriedCargo: 0, 
            bitcoin: 0,
            strain: 0
        };
    },
};