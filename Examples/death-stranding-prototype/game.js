const Game = {
    mapSize: 30,
    player: { x: 15, y: 15, moveCount: 0 },
    carriedCargo: 0,
    bitcoin: 0,
    strain: 0,
    terrain: [],
    tasks: [],
    worldMap: { width: 7, height: 7, currentX: 3, currentY: 3 },
    taskPage: 0,
    tasksPerPage: 3,
  
    init() {
      const { cities, citySymbols } = CityModule.initCities(3, this.worldMap.width, this.worldMap.height);
      this.cities = cities;
      this.citySymbols = citySymbols;
      this.generateTerrain();
      this.render();
      this.setupKeyboardControls();
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
      for (let y = 0; y < this.mapSize; y++) {
        this.terrain[y] = [];
        for (let x = 0; x < this.mapSize; x++) {
          if (heightMap[y][x] > 0.6) {
            this.terrain[y][x] = '^';
          } else {
            this.terrain[y][x] = '.';
          }
        }
      }
      const riverCount = 2;
      for (let i = 0; i < riverCount; i++) {
        let rx = Math.floor(Math.random() * this.mapSize);
        let ry = Math.floor(Math.random() * this.mapSize);
        this.carveRiver(rx, ry, heightMap);
      }
      if (CityModule.isCityChunk(this.worldMap.currentX, this.worldMap.currentY)) {
        this.currentCityBuildings = CityModule.injectCityLayout(this.terrain, this.mapSize);
      }
    },
  
    carveRiver(x, y, heightMap) {
      let steps = 0;
      while (true) {
        this.terrain[y][x] = '~';
        steps++;
        if (steps > 200) break;
        let currentH = heightMap[y][x];
        let foundLower = false;
        let nextPos = { x, y };
        const dirs = [{dx:1,dy:0},{dx:-1,dy:0},{dx:0,dy:1},{dx:0,dy:-1}];
        for (let d = dirs.length - 1; d > 0; d--) {
          const r = Math.floor(Math.random() * (d + 1));
          [dirs[d], dirs[r]] = [dirs[r], dirs[d]];
        }
        for (let dir of dirs) {
          let nx = x + dir.dx;
          let ny = y + dir.dy;
          if (nx < 0 || nx >= this.mapSize || ny < 0 || ny >= this.mapSize) return;
          if (heightMap[ny][nx] < currentH) {
            foundLower = true;
            nextPos.x = nx;
            nextPos.y = ny;
            break;
          }
        }
        if (!foundLower) return;
        x = nextPos.x;
        y = nextPos.y;
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
            } else if (tile === '✳') {
              display += '<span class="tree">✳</span>';
            } else if (CityModule.isCityChunk(this.worldMap.currentX, this.worldMap.currentY) && tile === '.') {
              display += '<span class="city-ground">.</span>';
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
        const start = this.taskPage * this.tasksPerPage;
        const end = start + this.tasksPerPage;
        const tasksToShow = this.tasks.slice(start, end);
        tasksToShow.forEach((task, index) => {
          const fromKey = `${task.fromCity.worldX},${task.fromCity.worldY}`;
          const toKey = `${task.toCity.worldX},${task.toCity.worldY}`;
          const fromSymbol = this.citySymbols[fromKey] || "C";
          const toSymbol = this.citySymbols[toKey] || "C";
          panel += `Task ${start + index + 1}: From [${fromSymbol}] to [${toSymbol}] - Cargo: ${task.cargo}<br>`;
        });
        const totalPages = Math.ceil(this.tasks.length / this.tasksPerPage);
        panel += `【Page ${this.taskPage + 1} / ${totalPages}】`;
      }
      document.getElementById('task-panel').innerHTML = panel;
    },
  
    isTilePassable(tile) {
      if (tile === '#') return false;
      if (tile === '✳') return false;
      if (/^[A-Za-z]+$/.test(tile) && tile !== 'O') return false;
      if (tile === '|' || tile === '.' || tile === '-' || tile === '■' || tile === '□') return true;
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
        if ((currentTile === '^' || currentTile === '~' || nextTile === '^' || nextTile === '~') && this.player.moveCount < 2) {
          if (this.carriedCargo > 0 && (nextTile === '^' || nextTile === '~') && Math.random() < 0.3) {
            this.strain = Math.min(this.strain + 10, 100);
          }
        } else {
          this.player.x = newX;
          this.player.y = newY;
          this.player.moveCount = 0;
          if (this.carriedCargo > 0) {
            if (nextTile === '^' || nextTile === '~') {
              if (Math.random() < 0.3) this.strain = Math.min(this.strain + 10, 100);
            }
          }
        }
        if (this.strain >= 100 && this.carriedCargo > 0 && this.tasks.length > 0) {
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
        switch(e.key) {
          case 'ArrowUp': case 'w': case 'W': this.movePlayer(0, -1); break;
          case 'ArrowDown': case 's': case 'S': this.movePlayer(0, 1); break;
          case 'ArrowLeft': case 'a': case 'A': this.movePlayer(-1, 0); break;
          case 'ArrowRight': case 'd': case 'D': this.movePlayer(1, 0); break;
          case ' ': case 'e': case 'E': this.activateAction(); break;
        }
      });
    },
  
    render() {
      this.renderWorldMap();
      this.renderWorldMiniMap();
      this.renderTaskPanel();
      document.getElementById('cargo-count').textContent = this.carriedCargo;
      document.getElementById('bitcoin').textContent = this.bitcoin.toFixed(2);
      document.getElementById('progress-fill').style.width = `${this.strain}%`;
    }
  };