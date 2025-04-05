const Game = {
    mapSize: GameConfig.worldGen.mapSize,
    player: { x: 15, y: 15 },
    tasks: [],
    carriedCargo: 0,
    bitcoin: 0,
    strain: 0,
    worldMap: {
        currentX: 0,
        currentY: 0,
        width: GameConfig.worldGen.worldWidth,
        height: GameConfig.worldGen.worldHeight
    },
    cities: [],
    citySymbols: {},
    keyState: {},

    init() {
        // Initialize cities
        const { cities, citySymbols } = CityModule.initCities(
            GameConfig.worldGen.cityCount, 
            this.worldMap.width, 
            this.worldMap.height, 
            this.worldMap
        );
        this.cities = cities;
        this.citySymbols = citySymbols;
        
        this.generateTerrain();
        this.setupKeyboardControls();
        this.render();
    },

    generateTerrain() {
      this.terrain = [];
      const seed = Math.random() * 1000;
      let heightMap = [];
      for (let y = 0; y < this.mapSize; y++) {
        heightMap[y] = [];
        for (let x = 0; x < this.mapSize; x++) {
          const val = smoothNoise(x / 10, y / 10, seed);
          heightMap[y][x] = val;
        }
      }
      
      // 先初始化全部为平地
      for (let y = 0; y < this.mapSize; y++) {
        this.terrain[y] = [];
        for (let x = 0; x < this.mapSize; x++) {
          this.terrain[y][x] = '.';
        }
      }
      
      // 判断当前区块是否为城市
      const isCity = CityModule.isCityChunk(this.worldMap.currentX, this.worldMap.currentY);
      
      // 如果是城市区块，先注入城市布局，获取城市边界
      let cityBounds = { minX: 0, minY: 0, maxX: this.mapSize, maxY: this.mapSize };
      if (isCity) {
        // 预计算城市区域边界
        const citySize = GameConfig.worldGen.citySize;
        const startX = Math.floor((this.mapSize - citySize) / 2);
        const startY = Math.floor((this.mapSize - citySize) / 2);
        cityBounds = {
          minX: startX,
          minY: startY,
          maxX: startX + citySize - 1,
          maxY: startY + citySize - 1
        };
      }
      
      // 生成山脉，但避开城市区域
      for (let y = 0; y < this.mapSize; y++) {
        for (let x = 0; x < this.mapSize; x++) {
          // 如果是城市区块且在城市边界内，保持平地
          if (isCity && x >= cityBounds.minX && x <= cityBounds.maxX && 
              y >= cityBounds.minY && y <= cityBounds.maxY) {
            continue;
          }
          
          if (heightMap[y][x] > GameConfig.worldGen.mountainThreshold) {
            this.terrain[y][x] = '^';
          } else if (heightMap[y][x] > GameConfig.worldGen.grassThreshold && Math.random() < GameConfig.worldGen.grassChance) {
            // 生成草地
            this.terrain[y][x] = '*';
          }
        }
      }
      
      // 生成河流，但避开城市区域
      const riverCount = GameConfig.worldGen.riverCount;
      for (let i = 0; i < riverCount; i++) {
        let rx, ry;
        // 避免河流起点位于城市区域内
        do {
          rx = Math.floor(Math.random() * this.mapSize);
          ry = Math.floor(Math.random() * this.mapSize);
        } while (isCity && rx >= cityBounds.minX && rx <= cityBounds.maxX && 
                 ry >= cityBounds.minY && ry <= cityBounds.maxY);
        
        this.carveRiver(rx, ry, heightMap, isCity, cityBounds);
      }
      
      // 生成树木，但数量稀少且避开城市区域
      this.generateTrees(isCity, cityBounds);
      
      // 如果是城市区块，注入城市布局
      if (isCity) {
        this.currentCityBuildings = CityModule.injectCityLayout(this.terrain, this.mapSize);
      }
    },
  
    carveRiver(x, y, heightMap, isCity, cityBounds) {
      // 设置第一个河流点
      if (isCity && x >= cityBounds.minX && x <= cityBounds.maxX && 
          y >= cityBounds.minY && y <= cityBounds.maxY) {
        return;
      }
      
      this.terrain[y][x] = '~';
      
      // 在第一格时就100%选择一个方向进行延伸
      // 随机选择一个方向: 0=右, 1=上, 2=左, 3=下
      const direction = Math.floor(Math.random() * 4);
      const dx = [1, 0, -1, 0][direction];
      const dy = [0, -1, 0, 1][direction];
      
      // 随机确定长度，从最小长度到地图边界不等
      const minLength = GameConfig.worldGen.riverMinLength;
      const maxLength = Math.max(this.mapSize - x, this.mapSize - y, x, y);
      const length = minLength + Math.floor(Math.random() * (maxLength - minLength + 1));
      
      // 沿着选定方向延伸河流
      for (let i = 1; i <= length; i++) {
        const nx = x + dx * i;
        const ny = y + dy * i;
        
        // 检查边界
        if (nx < 0 || nx >= this.mapSize || ny < 0 || ny >= this.mapSize) {
          break;
        }
        
        // 避免流入城市
        if (isCity && nx >= cityBounds.minX && nx <= cityBounds.maxX && 
            ny >= cityBounds.minY && ny <= cityBounds.maxY) {
          break;
        }
        
        this.terrain[ny][nx] = '~';
        
        // 随机分支河流
        if (Math.random() < GameConfig.worldGen.riverBranchChance) {
          this.branchRiver(nx, ny, dx, dy, isCity, cityBounds);
        }
      }
    },
    
    // 生成河流分支
    branchRiver(x, y, parentDx, parentDy, isCity, cityBounds) {
      // 选择与父河流不同的方向
      let possibleDirs = [];
      for (let dir = 0; dir < 4; dir++) {
        const dx = [1, 0, -1, 0][dir];
        const dy = [0, -1, 0, 1][dir];
        
        // 确保不是父河流的反方向或相同方向
        if (!(dx === parentDx && dy === parentDy) && !(dx === -parentDx && dy === -parentDy)) {
          possibleDirs.push({dx, dy});
        }
      }
      
      if (possibleDirs.length === 0) return;
      
      // 随机选择一个可能的方向
      const randomDir = possibleDirs[Math.floor(Math.random() * possibleDirs.length)];
      const dx = randomDir.dx;
      const dy = randomDir.dy;
      
      // 随机长度，3-7格
      const length = 3 + Math.floor(Math.random() * 5);
      
      for (let i = 1; i <= length; i++) {
        const nx = x + dx * i;
        const ny = y + dy * i;
        
        // 检查边界
        if (nx < 0 || nx >= this.mapSize || ny < 0 || ny >= this.mapSize) {
          break;
        }
        
        // 避免流入城市
        if (isCity && nx >= cityBounds.minX && nx <= cityBounds.maxX && 
            ny >= cityBounds.minY && ny <= cityBounds.maxY) {
          break;
        }
        
        this.terrain[ny][nx] = '~';
      }
    },
  
    renderWorldMap() {
      let display = "";
      for (let y = 0; y < this.mapSize; y++) {
        for (let x = 0; x < this.mapSize; x++) {
          if (x === this.player.x && y === this.player.y) {
            display += '<span class="player">S</span>';
          } else {
            let tile = this.terrain[y][x];
            if (tile === '^') {
              display += '<span class="mountain">^</span>';
            } else if (tile === '~') {
              display += '<span class="water">~</span>';
            } else if (tile === '|') {
              display += '<span class="door">|</span>';
            } else if (tile === 'x') {
              display += '<span class="tree">x</span>';
            } else if (tile === '*') {
              display += '<span class="grass">*</span>';
            } else if (tile === '.') {
              if (CityModule.isCityChunk(this.worldMap.currentX, this.worldMap.currentY)) {
                display += '<span class="city-ground">.</span>';
              } else {
                display += '<span class="plain">.</span>';
              }
            } else {
              display += tile;
            }
          }
        }
        display += "\n";
      }
      document.getElementById('game-map').innerHTML = display;
    },
  
    renderWorldMiniMap() {
      let worldDisplay = "";
      for (let y = 0; y < this.worldMap.height; y++) {
        for (let x = 0; x < this.worldMap.width; x++) {
          if (x === this.worldMap.currentX && y === this.worldMap.currentY) {
            worldDisplay += '<span class="player">S</span>';
          } else if (this.cities.some(c => c.worldX === x && c.worldY === y)) {
            const key = `${x},${y}`;
            let symbol = this.citySymbols[key] || "C";
            let isDelivery = this.tasks.some(task =>
              task.toCity.worldX === x && task.toCity.worldY === y
            );
            if (isDelivery) {
              worldDisplay += `<span class="delivery">${symbol}</span>`;
            } else {
              worldDisplay += `<span class="delivery">C</span>`;
            }
          } else {
            worldDisplay += ".";
          }
        }
        worldDisplay += "\n";
      }
      document.getElementById('world-map').innerHTML = worldDisplay;
    },
  
    renderTaskPanel() {
      let panel = "";
      if (this.tasks.length === 0) {
        panel = "No tasks available";
      } else {
        panel = "<strong>Current Tasks:</strong><br>";
        // 显示所有任务，不再分页
        this.tasks.forEach((task, index) => {
          const fromKey = `${task.fromCity.worldX},${task.fromCity.worldY}`;
          const toKey = `${task.toCity.worldX},${task.toCity.worldY}`;
          const fromSymbol = this.citySymbols[fromKey] || "C";
          const toSymbol = this.citySymbols[toKey] || "C";
          
          // 检查是否为目标城市，如果是则标记为特殊样式
          const isCurrent = (task.toCity.worldX === this.worldMap.currentX && 
                           task.toCity.worldY === this.worldMap.currentY);
          
          let taskClass = isCurrent ? 'delivery' : '';
          panel += `<span class="${taskClass}">Task ${index + 1}: From [${fromSymbol}] to [${toSymbol}] - Cargo: ${task.cargo}</span><br>`;
        });
      }
      document.getElementById('task-panel').innerHTML = panel;
    },
  
    isTilePassable(tile) {
      if (tile === '#') return false;
      if (tile === 'x') return false;
      if (/^[A-Za-z]+$/.test(tile) && tile !== 'O' && tile !== '*') return false;
      if (tile === '|' || tile === '.' || tile === '-' || tile === '■' || tile === '□' || tile === '*') return true;
      return true;
    },
  
    movePlayer(dx, dy) {
      let newX = this.player.x + dx;
      let newY = this.player.y + dy;
      if (newX < 0 || newX >= this.mapSize || newY < 0 || newY >= this.mapSize) {
        if (newX < 0 && this.worldMap.currentX > 0) {
          this.worldMap.currentX--;
          this.player.x = this.mapSize - 1;
          this.generateTerrain();
        } else if (newX >= this.mapSize && this.worldMap.currentX < this.worldMap.width - 1) {
          this.worldMap.currentX++;
          this.player.x = 0;
          this.generateTerrain();
        } else if (newY < 0 && this.worldMap.currentY > 0) {
          this.worldMap.currentY--;
          this.player.y = this.mapSize - 1;
          this.generateTerrain();
        } else if (newY >= this.mapSize && this.worldMap.currentY < this.worldMap.height - 1) {
          this.worldMap.currentY++;
          this.player.y = 0;
          this.generateTerrain();
        }
      } else {
        const nextTile = this.terrain[newY][newX];
        if (!this.isTilePassable(nextTile)) return;
        const currentTile = this.terrain[this.player.y][this.player.x];
        this.player.moveCount++;
        
        // 处理地形阻力（山脉和河流需要更多步数通过）
        const terrainResistance = 
          (currentTile === '^' || nextTile === '^') ? GameConfig.terrain.mountain.moveResistance : 
          (currentTile === '~' || nextTile === '~') ? GameConfig.terrain.river.moveResistance : 1;
        
        if (this.player.moveCount < terrainResistance) {
          // 还不能移动，但可能增加疲劳
          if (this.carriedCargo > 0) {
            if (nextTile === '^' && Math.random() < GameConfig.terrain.mountain.strainChance) {
              this.strain = Math.min(this.strain + GameConfig.terrain.mountain.strainAmount, GameConfig.player.maxStrain);
            } else if (nextTile === '~' && Math.random() < GameConfig.terrain.river.strainChance) {
              this.strain = Math.min(this.strain + GameConfig.terrain.river.strainAmount, GameConfig.player.maxStrain);
            }
          }
        } else {
          // 可以移动了
          this.player.x = newX;
          this.player.y = newY;
          this.player.moveCount = 0;
          
          // 移动后可能增加疲劳
          if (this.carriedCargo > 0) {
            if (nextTile === '^' && Math.random() < GameConfig.terrain.mountain.strainChance) {
              this.strain = Math.min(this.strain + GameConfig.terrain.mountain.strainAmount, GameConfig.player.maxStrain);
            } else if (nextTile === '~' && Math.random() < GameConfig.terrain.river.strainChance) {
              this.strain = Math.min(this.strain + GameConfig.terrain.river.strainAmount, GameConfig.player.maxStrain);
            }
          }
        }
        
        // 处理疲劳导致货物掉落
        if (this.strain >= GameConfig.player.maxStrain && this.carriedCargo > 0 && this.tasks.length > 0 && 
            Math.random() < GameConfig.mechanics.strainDropChance) {
          const randomTaskIndex = Math.floor(Math.random() * this.tasks.length);
          const failedTask = this.tasks[randomTaskIndex];
          
          alert(`You're too strained! The cargo for ${this.citySymbols[`${failedTask.toCity.worldX},${failedTask.toCity.worldY}`] || "city"} was dropped!`);
          
          this.tasks.splice(randomTaskIndex, 1);
          this.carriedCargo--;
          this.strain = 0;
        }
      }
      this.render();
    },
  
    activateAction() {
      if (!CityModule.isCityChunk(this.worldMap.currentX, this.worldMap.currentY)) {
        alert("Not in a city area, no interactive targets.");
        return;
      }
      const result = CityModule.handleCityAction(
        this.player,
        this.terrain,
        this.worldMap.currentX,
        this.worldMap.currentY,
        this.tasks,
        this.carriedCargo,
        this.bitcoin,
        this.strain
      );
      if (result.message) alert(result.message);
      if (result.acted) {
        this.tasks = result.tasks;
        this.carriedCargo = result.carriedCargo;
        this.bitcoin = result.bitcoin;
        this.strain = result.strain;
        this.render();
      }
    },
  
    nextTaskPage() {
      const totalPages = Math.ceil(this.tasks.length / this.tasksPerPage);
      if (this.taskPage < totalPages - 1) {
        this.taskPage++;
        this.renderTaskPanel();
      }
    },
  
    prevTaskPage() {
      if (this.taskPage > 0) {
        this.taskPage--;
        this.renderTaskPanel();
      }
    },
  
    setupKeyboardControls() {
      document.addEventListener('keydown', (e) => {
        e.preventDefault();
        this.keyState[e.key] = true;
        this.handleMovement();
      });

      document.addEventListener('keyup', (e) => {
        e.preventDefault();
        this.keyState[e.key] = false;
      });

      setInterval(() => {
        if (Object.values(this.keyState).some(state => state)) {
          this.handleMovement();
        }
      }, 100000);
    },
  
    handleMovement() {
      let dx = 0;
      let dy = 0;

      if ((this.keyState['ArrowUp'] || this.keyState['w'] || this.keyState['W']) && 
          (this.keyState['ArrowRight'] || this.keyState['d'] || this.keyState['D'])) {
        dx = 1; dy = -1;
      } else if ((this.keyState['ArrowUp'] || this.keyState['w'] || this.keyState['W']) && 
                 (this.keyState['ArrowLeft'] || this.keyState['a'] || this.keyState['A'])) {
        dx = -1; dy = -1;
      } else if ((this.keyState['ArrowDown'] || this.keyState['s'] || this.keyState['S']) && 
                 (this.keyState['ArrowRight'] || this.keyState['d'] || this.keyState['D'])) {
        dx = 1; dy = 1;
      } else if ((this.keyState['ArrowDown'] || this.keyState['s'] || this.keyState['S']) && 
                 (this.keyState['ArrowLeft'] || this.keyState['a'] || this.keyState['A'])) {
        dx = -1; dy = 1;
      } else {
        if (this.keyState['ArrowUp'] || this.keyState['w'] || this.keyState['W']) dy = -1;
        if (this.keyState['ArrowDown'] || this.keyState['s'] || this.keyState['S']) dy = 1;
        if (this.keyState['ArrowLeft'] || this.keyState['a'] || this.keyState['A']) dx = -1;
        if (this.keyState['ArrowRight'] || this.keyState['d'] || this.keyState['D']) dx = 1;
      }

      if (dx !== 0 || dy !== 0) {
        this.movePlayer(dx, dy);
      }
    },
  
    render() {
      this.renderWorldMap();
      this.renderWorldMiniMap();
      this.renderTaskPanel();
      document.getElementById('cargo-count').textContent = this.carriedCargo;
      document.getElementById('max-cargo').textContent = GameConfig.player.maxCargo;
      document.getElementById('bitcoin').textContent = this.bitcoin.toFixed(2);
      document.getElementById('progress-fill').style.width = `${this.strain}%`;
      document.getElementById('progress-label').textContent = `Strain: ${this.strain}%`;
    },

    generateTrees(isCity, cityBounds) {
      // 确定这张地图的树木总权重 (0-maxWeight)
      const totalTreeWeight = Math.floor(Math.random() * (GameConfig.worldGen.treeMaxWeight + 1));
      let currentWeight = 0;
      
      // 尝试放置树木直到达到目标权重
      let attempts = 0;
      const maxAttempts = 100; // 防止无限循环
      
      while (currentWeight < totalTreeWeight && attempts < maxAttempts) {
        attempts++;
        
        // 随机选择位置
        const x = Math.floor(Math.random() * this.mapSize);
        const y = Math.floor(Math.random() * this.mapSize);
        
        // 检查是否在城市内或已有其他地形
        if ((isCity && x >= cityBounds.minX && x <= cityBounds.maxX && 
             y >= cityBounds.minY && y <= cityBounds.maxY) ||
            this.terrain[y][x] !== '.' && this.terrain[y][x] !== '*') {
          continue;
        }
        
        // 决定生成小树(权重1)还是大树(权重4)
        const isLargeTree = Math.random() < GameConfig.worldGen.largeTreeChance && 
                           currentWeight <= totalTreeWeight - GameConfig.worldGen.largeTreeWeight;
        
        if (isLargeTree) {
          // 检查是否有足够空间放置大树(2x2)
          if (x + 1 >= this.mapSize || y + 1 >= this.mapSize) {
            continue;
          }
          
          // 检查2x2区域是否有其他障碍物
          let canPlaceLargeTree = true;
          for (let dy = 0; dy < 2; dy++) {
            for (let dx = 0; dx < 2; dx++) {
              const tx = x + dx;
              const ty = y + dy;
              if (this.terrain[ty][tx] !== '.' && this.terrain[ty][tx] !== '*') {
                canPlaceLargeTree = false;
                break;
              }
              // 确保不会放在城市内
              if (isCity && tx >= cityBounds.minX && tx <= cityBounds.maxX && 
                  ty >= cityBounds.minY && ty <= cityBounds.maxY) {
                canPlaceLargeTree = false;
                break;
              }
            }
            if (!canPlaceLargeTree) break;
          }
          
          if (canPlaceLargeTree) {
            // 放置2x2的大树
            for (let dy = 0; dy < 2; dy++) {
              for (let dx = 0; dx < 2; dx++) {
                this.terrain[y + dy][x + dx] = 'x'; // 大树用x表示
              }
            }
            currentWeight += GameConfig.worldGen.largeTreeWeight; // 大树权重为配置的值
          }
        } else {
          // 放置小树
          this.terrain[y][x] = 'x'; // 小树用x表示
          currentWeight += GameConfig.worldGen.smallTreeWeight; // 小树权重为配置的值
        }
      }
    }
  };