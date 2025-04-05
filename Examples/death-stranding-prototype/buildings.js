const BuildingModule = {
    // Room templates
    roomTemplates: {
        small: [
            ['#', '#', '#', '#'],
            ['#', '.', '.', '#'],
            ['#', '.', '.', '#'],
            ['#', '#', '#', '#']
        ],
        medium: [
            ['#', '#', '#', '#', '#'],
            ['#', '.', '.', '.', '#'],
            ['#', '.', '.', '.', '#'],
            ['#', '.', '.', '.', '#'],
            ['#', '#', '#', '#', '#']
        ],
        large: [
            ['#', '#', '#', '#', '#', '#'],
            ['#', '.', '.', '.', '.', '#'],
            ['#', '.', '.', '.', '.', '#'],
            ['#', '.', '.', '.', '.', '#'],
            ['#', '.', '.', '.', '.', '#'],
            ['#', '#', '#', '#', '#', '#']
        ],
        largeL: [
            ['#', '#', '#', '#', '#', '#'],
            ['#', '.', '.', '.', '.', '#'],
            ['#', '.', '.', '.', '.', '#'],
            ['#', '.', '.', '#', '#', '#'],
            ['#', '.', '.', '#', '.', '#'],
            ['#', '#', '#', '#', '.', '#']
        ]
    },

    // Special points that can be placed in rooms
    specialPoints: {
        task: 'T', // Task acceptance point
        delivery: 'D', // Delivery point
        rest: 'R', // Rest point
        storage: 'S' // Storage point
    },

    // Building types and their required special points
    buildingTypes: {
        bar: {
            name: 'Bar',
            requiredPoints: ['task'],
            optionalPoints: ['rest', 'storage'],
            roomSizes: ['small', 'medium']
        },
        yard: {
            name: 'Yard',
            requiredPoints: ['delivery'],
            optionalPoints: ['storage'],
            roomSizes: ['medium', 'large', 'largeL']
        },
        warehouse: {
            name: 'Warehouse',
            requiredPoints: ['storage'],
            optionalPoints: ['delivery'],
            roomSizes: ['large', 'largeL']
        },
        restStop: {
            name: 'Rest Stop',
            requiredPoints: ['rest'],
            optionalPoints: ['storage'],
            roomSizes: ['small', 'medium']
        }
    },

    // Generate a random building layout
    generateBuilding(type) {
        const buildingType = this.buildingTypes[type];
        if (!buildingType) return null;

        // Randomly select a room size
        const roomSize = buildingType.roomSizes[Math.floor(Math.random() * buildingType.roomSizes.length)];
        const template = this.roomTemplates[roomSize];

        // Create a copy of the template
        const layout = template.map(row => [...row]);

        // Place required special points
        for (const point of buildingType.requiredPoints) {
            this.placeSpecialPoint(layout, point);
        }

        // Place optional special points (50% chance for each)
        for (const point of buildingType.optionalPoints) {
            if (Math.random() < 0.5) {
                this.placeSpecialPoint(layout, point);
            }
        }

        return {
            type: type,
            name: buildingType.name,
            layout: layout,
            width: layout[0].length,
            height: layout.length
        };
    },

    // Place a special point in a valid position in the layout
    placeSpecialPoint(layout, pointType) {
        const validPositions = [];
        
        // Find all valid positions (empty floor tiles)
        for (let y = 1; y < layout.length - 1; y++) {
            for (let x = 1; x < layout[y].length - 1; x++) {
                if (layout[y][x] === '.') {
                    validPositions.push({x, y});
                }
            }
        }

        // If we found valid positions, place the point
        if (validPositions.length > 0) {
            const pos = validPositions[Math.floor(Math.random() * validPositions.length)];
            layout[pos.y][pos.x] = this.specialPoints[pointType];
        }
    },

    // Check if a position in a building is a special point
    isSpecialPoint(building, x, y) {
        if (!building || !building.layout) return false;
        const tile = building.layout[y][x];
        return Object.values(this.specialPoints).includes(tile);
    },

    // Get the type of special point at a position
    getSpecialPointType(building, x, y) {
        if (!building || !building.layout) return null;
        const tile = building.layout[y][x];
        for (const [type, symbol] of Object.entries(this.specialPoints)) {
            if (symbol === tile) return type;
        }
        return null;
    }
}; 