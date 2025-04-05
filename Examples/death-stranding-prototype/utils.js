// Noise Functions
function noise(x, y, seed) {
    const n = x + y * 57 + seed * 131;
    return (Math.sin(n * 12.9898) * 43758.5453) % 1;
  }
  
  function smoothNoise(x, y, seed) {
    const corners = (noise(x - 1, y - 1, seed) + noise(x + 1, y - 1, seed) +
                     noise(x - 1, y + 1, seed) + noise(x + 1, y + 1, seed)) / 16;
    const sides = (noise(x - 1, y, seed) + noise(x + 1, y, seed) +
                   noise(x, y - 1, seed) + noise(x, y + 1, seed)) / 8;
    const center = noise(x, y, seed) / 4;
    return corners + sides + center;
  }